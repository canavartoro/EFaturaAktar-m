﻿namespace EFaturaAktarim
{
    using EFatura.EFaturaWebService;
    using EFatura.Enums;
    using EFatura.Utility;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using NetOpenX.Rest.Client;
    using NetOpenX.Rest.Client.BLL;
    using NetOpenX.Rest.Client.Model;
    using NetOpenX.Rest.Client.Model.Enums;
    using NetOpenX.Rest.Client.Model.NetOpenX;
    using NetOpenX50;
    using Netsis.EFatura.DataObjects.Repository;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Serialization;

    public class MainForm : Form
    {
        private Button btnMigrateByDb;
        private Button btnMigrateByFolder;
        private Button btnSelectFolder;
        private ComboBox cmbFolderDbType;
        private ComboBox cmbSourceDbType;
        private ComboBox cmdDestDbType;
        private IContainer components = null;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label label1;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private Label label18;
        private Label label19;
        private Label label2;
        private Label label20;
        private Label label21;
        private Label label22;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private RadioButton rdIn;
        private RadioButton rdOut;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox txtDestinationBranch;
        private TextBox txtDestPassword;
        private TextBox txtDestSchema;
        private TextBox txtDestServer;
        private TextBox txtDestUser;
        private TextBox txtEnvelopeIds;
        private TextBox txtFolderBranch;
        private TextBox txtFolderPass;
        private TextBox txtFolderSchema;
        private TextBox txtFolderServer;
        private TextBox txtFolderUser;
        private TextBox txtSelectedFolder;
        private TextBox txtSourcePassword;
        private TextBox txtSourceSchema;
        private TextBox txtSourceServer;
        private CheckBox checkFatura;
        private Label label23;
        private TextBox textUrl;
        private Button btnaktar;
        private TextBox txtSourceUser;

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public MainForm()
        {
            this.InitializeComponent();
            this.cmbSourceDbType.SelectedIndex = 0;
            this.cmbFolderDbType.SelectedIndex = 0;
        }

        private void btnMigrateByDb_Click(object sender, EventArgs e)
        {
            string[] source = this.txtEnvelopeIds.Text.Split(Enumerable.ToArray<char>(Environment.NewLine), StringSplitOptions.RemoveEmptyEntries);
            if (Enumerable.Any<string>(source))
            {
                DbConnection connection;
                Exception exception;
                long ticks = DateTime.Now.Ticks;
                string path = string.Format("migrateByDb_{0}", ticks);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string str2 = Path.Combine(path, "log.txt");
                if (!File.Exists(str2))
                {
                    File.AppendAllText(str2, "----------------------------");
                    File.AppendAllText(str2, Environment.NewLine);
                }
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder["user id"] = this.txtSourceUser.Text;
                builder["password"] = this.txtSourcePassword.Text;
                builder["Data Source"] = this.txtSourceServer.Text;
                if (this.cmbSourceDbType.SelectedIndex == 0)
                {
                    builder["initial catalog"] = this.txtSourceSchema.Text;
                }
                DbConnectionStringBuilder builder2 = new DbConnectionStringBuilder();
                builder2["user id"] = this.txtDestUser.Text;
                builder2["password"] = this.txtDestPassword.Text;
                builder2["Data Source"] = this.txtDestServer.Text;
                if (this.cmdDestDbType.SelectedIndex == 0)
                {
                    builder2["initial catalog"] = this.txtDestSchema.Text;
                }
                BaseRepository.Configure(builder.ConnectionString, (this.cmbSourceDbType.SelectedIndex == 0) ? string.Empty : this.txtSourceSchema.Text, (this.cmbSourceDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                try
                {
                    using (connection = BaseRepository.CreateConnection())
                    {
                        connection.Open();
                        connection.Close();
                    }
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    MessageBox.Show(string.Format("Kaynak veritabanı bağlantısında hata oluştu:{0}{1}", Environment.NewLine, exception), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                BaseRepository.Configure(builder2.ConnectionString, (this.cmdDestDbType.SelectedIndex == 0) ? string.Empty : this.txtDestSchema.Text, (this.cmdDestDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                try
                {
                    using (connection = BaseRepository.CreateConnection())
                    {
                        connection.Open();
                        connection.Close();
                    }
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                    MessageBox.Show(string.Format("Hedef veritabanı bağlantısında hata oluştu:{0}{1}", Environment.NewLine, exception), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (MessageBox.Show("Belirtilen t\x00fcm zarflar aktarılacaktır. Devam etmek istiyor musunuz?", "Uyarı", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.Cancel)
                {
                    foreach (string str3 in source)
                    {
                        int envelopeInckeyno = 0;
                        int num3 = 0;
                        try
                        {
                            BaseRepository.Configure(builder.ConnectionString, (this.cmbSourceDbType.SelectedIndex == 0) ? string.Empty : this.txtSourceSchema.Text, (this.cmbSourceDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                            using (connection = BaseRepository.CreateConnection())
                            {
                                connection.Open();
                                DbCommand command = connection.CreateCommand();
                                command.CommandText = "SELECT INCKEYNO,TIP FROM {0}TBLEFATZARF WHERE ZARFID=@ZARFID";
                                DbParameter parameter = command.CreateParameter();
                                parameter.ParameterName = "ZARFID";
                                parameter.Value = str3;
                                parameter.DbType = DbType.String;
                                command.Parameters.Add(parameter);
                                BaseRepository.FixCommand(command, false);
                                DbDataReader reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    envelopeInckeyno = Convert.ToInt32(reader["INCKEYNO"]);
                                    num3 = Convert.ToByte(reader["TIP"]);
                                }
                            }
                            if (envelopeInckeyno != 0)
                            {
                                string filePath = EnvelopeHelper.Instance.SaveEnvelopeZip(envelopeInckeyno, path);
                                BaseRepository.Configure(builder2.ConnectionString, (this.cmdDestDbType.SelectedIndex == 0) ? string.Empty : this.txtDestSchema.Text, (this.cmdDestDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                                string str5 = MD5Hash.GetMD5Hash(filePath);
                                documentType type2 = new documentType();
                                base64Binary binary = new base64Binary();
                                binary.Value = File.ReadAllBytes(filePath);
                                binary.contentType = "application/zip";
                                type2.binaryData = binary;
                                type2.fileName = Path.GetFileName(filePath);
                                type2.hash = str5;
                                documentType documentRequest = type2;
                                string str6 = EnvelopeHelper.Instance.ProcessDocumentWithoutResponse(documentRequest, (short)Convert.ToInt32(this.txtDestinationBranch.Text), "", DateTime.MinValue, StateCodeType.stEnvelopeHandledSuccessfully, (byte)num3);
                                if (string.IsNullOrEmpty(str6))
                                {
                                    File.AppendAllText(str2, string.Format("{0} zarf {1} başarı ile aktarıldı", str3, (num3 == 1) ? "Giden dosya olarak" : "Gelen dosya olarak"));
                                }
                                else if (str6.StartsWith("Netsis.EFatura.Service.ExistingEnvelopeIdException", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    File.AppendAllText(str2, string.Format("{0} zarf hedef veritabanında bulundugu icin aktarılamadı", str3));
                                }
                                else
                                {
                                    File.AppendAllText(str2, string.Format("{0} zarf aktarımı sırasında hata olustu: {1}", str3, str6));
                                }
                                File.AppendAllText(str2, Environment.NewLine);
                            }
                        }
                        catch (Exception exception2)
                        {
                            File.AppendAllText(str2, string.Format("{0} nolu zarf aktarımında hata oluştu:{1}", str3, exception2));
                            File.AppendAllText(str2, Environment.NewLine);
                        }
                    }
                    Process.Start(str2);
                    MessageBox.Show("İşlem tamamlandı");
                }
            }
        }

        private void btnMigrateByFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSelectedFolder.Text))
            {
                string[] files;
                Exception exception;
                try
                {
                    files = Directory.GetFiles(this.txtSelectedFolder.Text, "*.zip");
                    if (files.Length == 0)
                    {
                        return;
                    }
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    MessageBox.Show(string.Format("Dosyalar alınırken hata oluştu:{0}{1}", Environment.NewLine, exception), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (MessageBox.Show(string.Format("İlgili folder altındaki {0} dosya {1} olarak aktarılacaktır. Devam etmek istiyor musunuz?", files.Length, this.rdIn.Checked ? "alış faturası" : "satış faturası"), "Uyarı", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.Cancel)
                {
                    long ticks = DateTime.Now.Ticks;
                    string path = string.Format("migrateByFolder_{0}", ticks);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string str2 = Path.Combine(path, "log.txt");
                    if (!File.Exists(str2))
                    {
                        File.AppendAllText(str2, "----------------------------");
                        File.AppendAllText(str2, Environment.NewLine);
                    }
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder["user id"] = this.txtFolderUser.Text;
                    builder["password"] = this.txtFolderPass.Text;
                    builder["Data Source"] = this.txtFolderServer.Text;
                    if (this.cmbFolderDbType.SelectedIndex == 0)
                    {
                        builder["initial catalog"] = this.txtFolderSchema.Text;
                    }
                    BaseRepository.Configure(builder.ConnectionString, (this.cmbFolderDbType.SelectedIndex == 0) ? string.Empty : this.txtFolderSchema.Text, (this.cmbFolderDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                    try
                    {
                        using (DbConnection connection = BaseRepository.CreateConnection())
                        {
                            connection.Open();
                            connection.Close();
                        }
                    }
                    catch (Exception exception2)
                    {
                        MessageBox.Show(string.Format("Hedef veritabanı bağlantısında hata oluştu:{0}{1}", Environment.NewLine, exception2), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                    foreach (string str3 in files)
                    {
                        try
                        {
                            string str4 = MD5Hash.GetMD5Hash(str3);
                            documentType type2 = new documentType();
                            base64Binary binary = new base64Binary();
                            binary.Value = File.ReadAllBytes(str3);
                            binary.contentType = "application/zip";
                            type2.binaryData = binary;
                            type2.fileName = Path.GetFileName(str3);
                            type2.hash = str4;
                            documentType documentRequest = type2;
                            string str5 = EnvelopeHelper.Instance.ProcessDocumentWithoutResponse(documentRequest, (short)Convert.ToInt32(this.txtFolderBranch.Text), "", DateTime.MinValue, StateCodeType.stEnvelopeHandledSuccessfully, this.rdIn.Checked ? ((byte)2) : ((byte)1));
                            if (string.IsNullOrEmpty(str5))
                            {
                                File.AppendAllText(str2, string.Format("{0} zarf başarı ile aktarıldı", str3));
                            }
                            else if (str5.StartsWith("Netsis.EFatura.Service.ExistingEnvelopeIdException", StringComparison.InvariantCultureIgnoreCase))
                            {
                                File.AppendAllText(str2, string.Format("{0} zarf hedef veritabanında bulundugu icin aktarılamadı", str3));
                            }
                            else
                            {
                                File.AppendAllText(str2, string.Format("{0} zarf aktarımı sırasında hata olustu: {1}", str3, str5));
                            }
                            File.AppendAllText(str2, Environment.NewLine);
                        }
                        catch (Exception exception4)
                        {
                            exception = exception4;
                            File.AppendAllText(str2, string.Format("{0} nolu zarf aktarımında hata oluştu:{1}", str3, exception));
                            File.AppendAllText(str2, Environment.NewLine);
                        }
                    }
                    Process.Start(str2);
                    MessageBox.Show("İşlem tamamlandı");
                }
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txtSelectedFolder.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void cmbSourceDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.cmdDestDbType.SelectedIndex = this.cmbSourceDbType.SelectedIndex;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnMigrateByDb = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.txtDestinationBranch = new System.Windows.Forms.TextBox();
            this.txtEnvelopeIds = new System.Windows.Forms.TextBox();
            this.cmdDestDbType = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDestPassword = new System.Windows.Forms.TextBox();
            this.txtDestUser = new System.Windows.Forms.TextBox();
            this.txtDestSchema = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtDestServer = new System.Windows.Forms.TextBox();
            this.cmbSourceDbType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSourcePassword = new System.Windows.Forms.TextBox();
            this.txtSourceUser = new System.Windows.Forms.TextBox();
            this.txtSourceSchema = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSourceServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkFatura = new System.Windows.Forms.CheckBox();
            this.rdOut = new System.Windows.Forms.RadioButton();
            this.label22 = new System.Windows.Forms.Label();
            this.rdIn = new System.Windows.Forms.RadioButton();
            this.btnMigrateByFolder = new System.Windows.Forms.Button();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.label21 = new System.Windows.Forms.Label();
            this.txtSelectedFolder = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtFolderBranch = new System.Windows.Forms.TextBox();
            this.cmbFolderDbType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtFolderPass = new System.Windows.Forms.TextBox();
            this.txtFolderUser = new System.Windows.Forms.TextBox();
            this.txtFolderSchema = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.txtFolderServer = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textUrl = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.btnaktar = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(741, 435);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnMigrateByDb);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.txtDestinationBranch);
            this.tabPage1.Controls.Add(this.txtEnvelopeIds);
            this.tabPage1.Controls.Add(this.cmdDestDbType);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.txtDestPassword);
            this.tabPage1.Controls.Add(this.txtDestUser);
            this.tabPage1.Controls.Add(this.txtDestSchema);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.txtDestServer);
            this.tabPage1.Controls.Add(this.cmbSourceDbType);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.txtSourcePassword);
            this.tabPage1.Controls.Add(this.txtSourceUser);
            this.tabPage1.Controls.Add(this.txtSourceSchema);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtSourceServer);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage1.Size = new System.Drawing.Size(733, 409);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Veritabanından Aktarım";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnMigrateByDb
            // 
            this.btnMigrateByDb.Location = new System.Drawing.Point(602, 382);
            this.btnMigrateByDb.Name = "btnMigrateByDb";
            this.btnMigrateByDb.Size = new System.Drawing.Size(100, 23);
            this.btnMigrateByDb.TabIndex = 45;
            this.btnMigrateByDb.Text = "Aktar";
            this.btnMigrateByDb.UseVisualStyleBackColor = true;
            this.btnMigrateByDb.Click += new System.EventHandler(this.btnMigrateByDb_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(599, 109);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 52;
            this.label13.Text = "Şube Kodu";
            // 
            // txtDestinationBranch
            // 
            this.txtDestinationBranch.Location = new System.Drawing.Point(602, 128);
            this.txtDestinationBranch.Name = "txtDestinationBranch";
            this.txtDestinationBranch.Size = new System.Drawing.Size(100, 20);
            this.txtDestinationBranch.TabIndex = 43;
            // 
            // txtEnvelopeIds
            // 
            this.txtEnvelopeIds.Location = new System.Drawing.Point(8, 167);
            this.txtEnvelopeIds.Multiline = true;
            this.txtEnvelopeIds.Name = "txtEnvelopeIds";
            this.txtEnvelopeIds.Size = new System.Drawing.Size(584, 239);
            this.txtEnvelopeIds.TabIndex = 44;
            // 
            // cmdDestDbType
            // 
            this.cmdDestDbType.FormattingEnabled = true;
            this.cmdDestDbType.Items.AddRange(new object[] {
            "Sql",
            "Oracle"});
            this.cmdDestDbType.Location = new System.Drawing.Point(7, 128);
            this.cmdDestDbType.Name = "cmdDestDbType";
            this.cmdDestDbType.Size = new System.Drawing.Size(89, 21);
            this.cmdDestDbType.TabIndex = 34;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 51;
            this.label8.Text = "Veritabanı Tipi";
            // 
            // txtDestPassword
            // 
            this.txtDestPassword.Location = new System.Drawing.Point(474, 128);
            this.txtDestPassword.Name = "txtDestPassword";
            this.txtDestPassword.PasswordChar = '*';
            this.txtDestPassword.Size = new System.Drawing.Size(118, 20);
            this.txtDestPassword.TabIndex = 42;
            // 
            // txtDestUser
            // 
            this.txtDestUser.Location = new System.Drawing.Point(350, 128);
            this.txtDestUser.Name = "txtDestUser";
            this.txtDestUser.Size = new System.Drawing.Size(118, 20);
            this.txtDestUser.TabIndex = 40;
            // 
            // txtDestSchema
            // 
            this.txtDestSchema.Location = new System.Drawing.Point(226, 128);
            this.txtDestSchema.Name = "txtDestSchema";
            this.txtDestSchema.Size = new System.Drawing.Size(118, 20);
            this.txtDestSchema.TabIndex = 39;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(223, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 13);
            this.label9.TabIndex = 50;
            this.label9.Text = "Veritabanı Adı";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(99, 109);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 49;
            this.label10.Text = "Sunucu Adı";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(471, 109);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(28, 13);
            this.label11.TabIndex = 48;
            this.label11.Text = "Şifre";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(347, 109);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(64, 13);
            this.label12.TabIndex = 47;
            this.label12.Text = "Kullanıcı Adı";
            // 
            // txtDestServer
            // 
            this.txtDestServer.Location = new System.Drawing.Point(102, 128);
            this.txtDestServer.Name = "txtDestServer";
            this.txtDestServer.Size = new System.Drawing.Size(118, 20);
            this.txtDestServer.TabIndex = 37;
            // 
            // cmbSourceDbType
            // 
            this.cmbSourceDbType.FormattingEnabled = true;
            this.cmbSourceDbType.Items.AddRange(new object[] {
            "Sql",
            "Oracle"});
            this.cmbSourceDbType.Location = new System.Drawing.Point(8, 53);
            this.cmbSourceDbType.Name = "cmbSourceDbType";
            this.cmbSourceDbType.Size = new System.Drawing.Size(89, 21);
            this.cmbSourceDbType.TabIndex = 28;
            this.cmbSourceDbType.SelectedIndexChanged += new System.EventHandler(this.cmbSourceDbType_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 46;
            this.label7.Text = "Veritabanı Tipi";
            // 
            // txtSourcePassword
            // 
            this.txtSourcePassword.Location = new System.Drawing.Point(475, 53);
            this.txtSourcePassword.Name = "txtSourcePassword";
            this.txtSourcePassword.PasswordChar = '*';
            this.txtSourcePassword.Size = new System.Drawing.Size(118, 20);
            this.txtSourcePassword.TabIndex = 33;
            this.txtSourcePassword.Leave += new System.EventHandler(this.txtSourcePassword_Leave);
            // 
            // txtSourceUser
            // 
            this.txtSourceUser.Location = new System.Drawing.Point(351, 53);
            this.txtSourceUser.Name = "txtSourceUser";
            this.txtSourceUser.Size = new System.Drawing.Size(118, 20);
            this.txtSourceUser.TabIndex = 32;
            this.txtSourceUser.Leave += new System.EventHandler(this.txtSourceUser_Leave);
            // 
            // txtSourceSchema
            // 
            this.txtSourceSchema.Location = new System.Drawing.Point(227, 53);
            this.txtSourceSchema.Name = "txtSourceSchema";
            this.txtSourceSchema.Size = new System.Drawing.Size(118, 20);
            this.txtSourceSchema.TabIndex = 31;
            this.txtSourceSchema.Leave += new System.EventHandler(this.txtSourceSchema_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(224, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 41;
            this.label6.Text = "Veritabanı Adı";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(100, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "Sunucu Adı";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(472, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 36;
            this.label4.Text = "Şifre";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(348, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Kullanıcı Adı";
            // 
            // txtSourceServer
            // 
            this.txtSourceServer.Location = new System.Drawing.Point(103, 53);
            this.txtSourceServer.Name = "txtSourceServer";
            this.txtSourceServer.Size = new System.Drawing.Size(118, 20);
            this.txtSourceServer.TabIndex = 29;
            this.txtSourceServer.Leave += new System.EventHandler(this.txtSourceServer_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(5, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Hedef Veritabanı Bilgileri";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Kaynak Veritabanı Bilgileri";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnaktar);
            this.tabPage2.Controls.Add(this.label23);
            this.tabPage2.Controls.Add(this.textUrl);
            this.tabPage2.Controls.Add(this.checkFatura);
            this.tabPage2.Controls.Add(this.rdOut);
            this.tabPage2.Controls.Add(this.label22);
            this.tabPage2.Controls.Add(this.rdIn);
            this.tabPage2.Controls.Add(this.btnMigrateByFolder);
            this.tabPage2.Controls.Add(this.btnSelectFolder);
            this.tabPage2.Controls.Add(this.label21);
            this.tabPage2.Controls.Add(this.txtSelectedFolder);
            this.tabPage2.Controls.Add(this.label14);
            this.tabPage2.Controls.Add(this.txtFolderBranch);
            this.tabPage2.Controls.Add(this.cmbFolderDbType);
            this.tabPage2.Controls.Add(this.label15);
            this.tabPage2.Controls.Add(this.txtFolderPass);
            this.tabPage2.Controls.Add(this.txtFolderUser);
            this.tabPage2.Controls.Add(this.txtFolderSchema);
            this.tabPage2.Controls.Add(this.label16);
            this.tabPage2.Controls.Add(this.label17);
            this.tabPage2.Controls.Add(this.label18);
            this.tabPage2.Controls.Add(this.label19);
            this.tabPage2.Controls.Add(this.txtFolderServer);
            this.tabPage2.Controls.Add(this.label20);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage2.Size = new System.Drawing.Size(733, 409);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Klasörden Aktarım";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkFatura
            // 
            this.checkFatura.AutoSize = true;
            this.checkFatura.Location = new System.Drawing.Point(14, 170);
            this.checkFatura.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkFatura.Name = "checkFatura";
            this.checkFatura.Size = new System.Drawing.Size(202, 17);
            this.checkFatura.TabIndex = 72;
            this.checkFatura.Text = "Netopenx ile fatura kaydı oluşturulsun";
            this.checkFatura.UseVisualStyleBackColor = true;
            // 
            // rdOut
            // 
            this.rdOut.AutoSize = true;
            this.rdOut.Location = new System.Drawing.Point(61, 108);
            this.rdOut.Name = "rdOut";
            this.rdOut.Size = new System.Drawing.Size(48, 17);
            this.rdOut.TabIndex = 61;
            this.rdOut.Text = "Satış";
            this.rdOut.UseVisualStyleBackColor = true;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(11, 91);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(57, 13);
            this.label22.TabIndex = 71;
            this.label22.Text = "Fatura Tipi";
            // 
            // rdIn
            // 
            this.rdIn.AutoSize = true;
            this.rdIn.Checked = true;
            this.rdIn.Location = new System.Drawing.Point(14, 107);
            this.rdIn.Name = "rdIn";
            this.rdIn.Size = new System.Drawing.Size(41, 17);
            this.rdIn.TabIndex = 60;
            this.rdIn.TabStop = true;
            this.rdIn.Text = "Alış";
            this.rdIn.UseVisualStyleBackColor = true;
            // 
            // btnMigrateByFolder
            // 
            this.btnMigrateByFolder.Location = new System.Drawing.Point(487, 102);
            this.btnMigrateByFolder.Name = "btnMigrateByFolder";
            this.btnMigrateByFolder.Size = new System.Drawing.Size(75, 23);
            this.btnMigrateByFolder.TabIndex = 69;
            this.btnMigrateByFolder.Text = "Aktar";
            this.btnMigrateByFolder.UseVisualStyleBackColor = true;
            this.btnMigrateByFolder.Click += new System.EventHandler(this.btnMigrateByFolder_Click);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(447, 102);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(34, 23);
            this.btnSelectFolder.TabIndex = 68;
            this.btnSelectFolder.Text = "...";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(112, 88);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(70, 13);
            this.label21.TabIndex = 67;
            this.label21.Text = "Klasör Seçimi";
            // 
            // txtSelectedFolder
            // 
            this.txtSelectedFolder.Location = new System.Drawing.Point(115, 104);
            this.txtSelectedFolder.Name = "txtSelectedFolder";
            this.txtSelectedFolder.Size = new System.Drawing.Size(325, 20);
            this.txtSelectedFolder.TabIndex = 66;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(608, 34);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(60, 13);
            this.label14.TabIndex = 65;
            this.label14.Text = "Şube Kodu";
            // 
            // txtFolderBranch
            // 
            this.txtFolderBranch.Location = new System.Drawing.Point(611, 53);
            this.txtFolderBranch.Name = "txtFolderBranch";
            this.txtFolderBranch.Size = new System.Drawing.Size(100, 20);
            this.txtFolderBranch.TabIndex = 59;
            // 
            // cmbFolderDbType
            // 
            this.cmbFolderDbType.FormattingEnabled = true;
            this.cmbFolderDbType.Items.AddRange(new object[] {
            "Sql",
            "Oracle"});
            this.cmbFolderDbType.Location = new System.Drawing.Point(10, 53);
            this.cmbFolderDbType.Name = "cmbFolderDbType";
            this.cmbFolderDbType.Size = new System.Drawing.Size(99, 21);
            this.cmbFolderDbType.TabIndex = 54;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 34);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(74, 13);
            this.label15.TabIndex = 64;
            this.label15.Text = "Veritabanı Tipi";
            // 
            // txtFolderPass
            // 
            this.txtFolderPass.Location = new System.Drawing.Point(487, 53);
            this.txtFolderPass.Name = "txtFolderPass";
            this.txtFolderPass.PasswordChar = '*';
            this.txtFolderPass.Size = new System.Drawing.Size(118, 20);
            this.txtFolderPass.TabIndex = 58;
            // 
            // txtFolderUser
            // 
            this.txtFolderUser.Location = new System.Drawing.Point(363, 53);
            this.txtFolderUser.Name = "txtFolderUser";
            this.txtFolderUser.Size = new System.Drawing.Size(118, 20);
            this.txtFolderUser.TabIndex = 57;
            // 
            // txtFolderSchema
            // 
            this.txtFolderSchema.Location = new System.Drawing.Point(239, 53);
            this.txtFolderSchema.Name = "txtFolderSchema";
            this.txtFolderSchema.Size = new System.Drawing.Size(118, 20);
            this.txtFolderSchema.TabIndex = 56;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(236, 34);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(72, 13);
            this.label16.TabIndex = 63;
            this.label16.Text = "Veritabanı Adı";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(112, 34);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(62, 13);
            this.label17.TabIndex = 62;
            this.label17.Text = "Sunucu Adı";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(484, 34);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(28, 13);
            this.label18.TabIndex = 61;
            this.label18.Text = "Şifre";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(360, 34);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(64, 13);
            this.label19.TabIndex = 60;
            this.label19.Text = "Kullanıcı Adı";
            // 
            // txtFolderServer
            // 
            this.txtFolderServer.Location = new System.Drawing.Point(115, 53);
            this.txtFolderServer.Name = "txtFolderServer";
            this.txtFolderServer.Size = new System.Drawing.Size(118, 20);
            this.txtFolderServer.TabIndex = 55;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label20.Location = new System.Drawing.Point(8, 12);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(147, 13);
            this.label20.TabIndex = 53;
            this.label20.Text = "Hedef Veritabanı Bilgileri";
            // 
            // textUrl
            // 
            this.textUrl.Location = new System.Drawing.Point(115, 135);
            this.textUrl.Name = "textUrl";
            this.textUrl.Size = new System.Drawing.Size(325, 20);
            this.textUrl.TabIndex = 73;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(71, 138);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(38, 13);
            this.label23.TabIndex = 74;
            this.label23.Text = "Api Url";
            // 
            // btnaktar
            // 
            this.btnaktar.Location = new System.Drawing.Point(447, 135);
            this.btnaktar.Name = "btnaktar";
            this.btnaktar.Size = new System.Drawing.Size(115, 23);
            this.btnaktar.TabIndex = 75;
            this.btnaktar.Text = "Aktar";
            this.btnaktar.UseVisualStyleBackColor = true;
            this.btnaktar.Click += new System.EventHandler(this.btnaktar_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 435);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "EFatura Aktarım";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        private void txtSourcePassword_Leave(object sender, EventArgs e)
        {
            this.txtDestPassword.Text = this.txtSourcePassword.Text;
        }

        private void txtSourceSchema_Leave(object sender, EventArgs e)
        {
        }

        private void txtSourceServer_Leave(object sender, EventArgs e)
        {
            this.txtDestServer.Text = this.txtSourceServer.Text;
        }

        private void txtSourceUser_Leave(object sender, EventArgs e)
        {
            this.txtDestUser.Text = this.txtSourceUser.Text;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //string file = @"D:\Projeler\Calismalar\MTA\EFaturaAktarım\EFaturaAktarim\bin\Debug\Envelope15042022121106.zip";

            //string xmlfile = @"D:\Projeler\Calismalar\MTA\EFaturaAktarım\EFaturaAktarim\bin\Debug\2869462e-8fc2-4d9e-bda8-10b0f59abe7a.xml";

            //XmlSerializer serializer = new XmlSerializer(typeof(StandardBusinessDocument));
            //using (FileStream fileStream = new FileStream(xmlfile, FileMode.Open))
            //{
            //    StandardBusinessDocument result = (StandardBusinessDocument)serializer.Deserialize(fileStream);
            //    if (result != null)
            //    {

            //    }
            //}


            //string str = EnvelopeHelper.Instance.GetInvoiceXmlFromEnvelopeXml(xmlfile, "2869462e-8fc2-4d9e-bda8-10b0f59abe7a");

            cmbFolderDbType.SelectedIndex = Convert.ToInt32(GetPrivateProfile("cmbFolderDbType", "0"));

            txtFolderServer.Text = GetPrivateProfile("txtFolderServer", "");

            txtFolderSchema.Text = GetPrivateProfile("txtFolderSchema", "");

            txtFolderUser.Text = GetPrivateProfile("txtFolderUser", "");

            txtFolderPass.Text = GetPrivateProfile("txtFolderPass", "");

            txtFolderBranch.Text = GetPrivateProfile("txtFolderBranch", "");

            txtSelectedFolder.Text = GetPrivateProfile("txtSelectedFolder", "");

            textUrl.Text = GetPrivateProfile("textUrl", "");

            rdIn.Checked = GetPrivateProfile("rdIn", "0") == "1";

            checkFatura.Checked = GetPrivateProfile("checkFatura", "0") == "1";


        }

        private void btnaktar_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter writer = null;
            try
            {
                SaveProfile();

                DirectoryInfo directoryInfo = new DirectoryInfo(txtSelectedFolder.Text); //new DirectoryInfo(txtSelectedFolder.Text);
                if (directoryInfo.Exists)
                {
                    DbConnection connection;
                    DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                    builder["user id"] = this.txtFolderUser.Text;
                    builder["password"] = this.txtFolderPass.Text;
                    builder["Data Source"] = this.txtFolderServer.Text;
                    if (this.cmbSourceDbType.SelectedIndex == 0)
                    {
                        builder["initial catalog"] = this.txtFolderSchema.Text;
                    }
                    BaseRepository.Configure(builder.ConnectionString, (this.cmbSourceDbType.SelectedIndex == 0) ? string.Empty : this.txtSourceSchema.Text, (this.cmbSourceDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
                    connection = BaseRepository.CreateConnection();
                    connection.Open();
                    DbCommand command = connection.CreateCommand();
                    var files = directoryInfo.GetFiles("*.zip");
                    if (files != null && files.Length > 0)
                    {
                        writer = new System.IO.StreamWriter($"{txtSelectedFolder.Text}\\aktarim.log", false, System.Text.Encoding.GetEncoding("windows-1254"));
                        foreach (var f in files)
                        {
                            //ICSharpCode.SharpZipLib.Zip.FastZip shpZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                            //shpZip.ExtractZip(f.FullName, Application.StartupPath, ".*");

                            FastZip fz = new FastZip();

                            using (ZipFile zipArchive = new ZipFile(f.FullName))
                            {
                                byte[] buffer = new byte[4096];
                                foreach (ZipEntry zipEntry in zipArchive)
                                {
                                    String fullZipToPath = Path.Combine(txtSelectedFolder.Text, zipEntry.Name);

                                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                                    if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
                                        Directory.CreateDirectory(directoryName);

                                    Stream zipStream = zipArchive.GetInputStream(zipEntry);

                                    using (FileStream streamWriter = File.Create(fullZipToPath))
                                    {
                                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                                    }

                                    if (zipEntry.Name.EndsWith(".zip"))
                                    {
                                        using (ZipFile zipArch = new ZipFile($"{txtSelectedFolder.Text}\\{zipEntry.Name}"))
                                        {
                                            foreach (ZipEntry zipEnt in zipArch)
                                            {
                                                String fullZipath = Path.Combine(directoryName, zipEnt.Name);

                                                string dirName = Path.GetDirectoryName(fullZipath);

                                                if (dirName.Length > 0)
                                                    Directory.CreateDirectory(dirName);

                                                Stream zipSt = zipArch.GetInputStream(zipEnt);
                                                using (FileStream streamWriter = File.Create(fullZipath))
                                                {
                                                    StreamUtils.Copy(zipSt, streamWriter, buffer);
                                                }
                                                ReadInvoiceXmlFile(fullZipath, command, writer);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ReadInvoiceXmlFile(zipEntry.Name, command, writer);
                                    }
                                }
                            }



                            //using (ZipInputStream s = new ZipInputStream(File.OpenRead(f.FullName)))
                            //{
                            //    ZipEntry theEntry;
                            //    while ((theEntry = s.GetNextEntry()) != null)
                            //    {
                            //        string directoryName = Path.GetDirectoryName(theEntry.Name);
                            //        string fileName = Path.GetFileName(theEntry.Name);

                            //        if (fileName != String.Empty)
                            //        {
                            //            using (FileStream streamWriter = File.Create(theEntry.Name))
                            //            {
                            //                int size = 2048;
                            //                byte[] data = new byte[2048];
                            //                while (true)
                            //                {
                            //                    size = s.Read(data, 0, data.Length);
                            //                    if (size > 0)
                            //                    {
                            //                        streamWriter.Write(data, 0, size);
                            //                    }
                            //                    else
                            //                    {
                            //                        break;
                            //                    }
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                writer.WriteLine($"Aktarim genel hata:{exc.Message} detay:{exc.StackTrace}");
                MessageBox.Show(string.Format("Hedef veritabanı bağlantısında hata oluştu:{0}{1}", Environment.NewLine, exc), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        private void ReadXmlFile(string xmlfile)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StandardBusinessDocument));
                using (FileStream fileStream = new FileStream(xmlfile, FileMode.Open))
                {
                    StandardBusinessDocument result = (StandardBusinessDocument)serializer.Deserialize(fileStream);
                    if (result != null)
                    {

                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Xml dosyasi acilirken hata:{0}{1}", Environment.NewLine, exc), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void ReadInvoiceXmlFile(string xmlfile, DbCommand command, StreamWriter logwriter)
        {
            try
            {
                //using (var reader = new XmlTextReader(xmlfile))
                //{
                //    reader.WhitespaceHandling = WhitespaceHandling.None;
                //    while (reader.Read())
                //    {
                //        switch (reader.NodeType)
                //        {
                //            case XmlNodeType.Element:
                //                Trace.Write(string.Format("<{0}>", reader.Name));
                //                break;
                //            case XmlNodeType.Text:
                //                Trace.Write(reader.Value);
                //                break;
                //            case XmlNodeType.CDATA:
                //                Trace.Write(string.Format("<![CDATA[{0}]]>", reader.Value));
                //                break;
                //            case XmlNodeType.ProcessingInstruction:
                //                Trace.Write(string.Format("<?{0} {1}?>", reader.Name, reader.Value));
                //                break;
                //            case XmlNodeType.Comment:
                //                Trace.Write(string.Format("<!--{0}-->", reader.Value));
                //                break;
                //            case XmlNodeType.XmlDeclaration:
                //                Trace.Write(string.Format("<?xml version='1.0'?>"));
                //                break;
                //            case XmlNodeType.Document:
                //                break;
                //            case XmlNodeType.DocumentType:
                //                Trace.Write(string.Format("<!DOCTYPE {0} [{1}]", reader.Name, reader.Value));
                //                break;
                //            case XmlNodeType.EntityReference:
                //                Trace.Write(reader.Name);
                //                break;
                //            case XmlNodeType.EndElement:
                //                Trace.Write(string.Format("</{0}>", reader.Name));
                //                break;
                //        }
                //    }
                //}

                XmlSerializer serializer = new XmlSerializer(typeof(UblTr.MainDoc.InvoiceType));
                using (StreamReader streamreader = new StreamReader(xmlfile))
                {
                    var invoice = (UblTr.MainDoc.InvoiceType)serializer.Deserialize(streamreader);
                    if (invoice != null)
                    {
                        string cariKod = "";
                        command.CommandText = "select c.CARI_KOD,VERGI_NUMARASI,ce.TCKIMLIKNO from tblcasabit as c inner join TBLCASABITEK as ce on c.CARI_KOD=ce.CARI_KOD where VERGI_NUMARASI = @TKN or ce.TCKIMLIKNO = @TKN";
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "TKN";
                        parameter.Value = invoice.AccountingCustomerParty.Party.AgentParty.PartyIdentification[0].ID.Value; //invoice.AccountingSupplierParty.Party.PartyIdentification[0].ID.Value;
                        parameter.DbType = DbType.String;
                        command.Parameters.Add(parameter);
                        BaseRepository.FixCommand(command, false);
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (reader != null && reader.Read())
                            {
                                cariKod = reader.GetValue(0).ToString();
                            }
                        }
                        command.Parameters.Clear();

                        command.CommandText = string.Format("SELECT COUNT(*) FROM TBLFATUIRS WITH (NOLOCK) WHERE FTIRSIP = '1' AND CARI_KODU = '{0}' AND FATIRS_NO = '{1}'", cariKod, invoice.ID.Value);
                        var checkFat = command.ExecuteScalar();
                        if (checkFat != null && Convert.ToInt32(checkFat) > 0)
                        {
                            logwriter.WriteLine($"{xmlfile} fatura var!");
                        }

                        Kernel kernel = new Kernel();
                        Sirket sirket = default(Sirket);
                        Fatura fatura = default(Fatura);
                        FatUst fatUst = default(FatUst);
                        FatKalem fatKalem = default(FatKalem);

                        try
                        {
                            sirket = kernel.yeniSirket(TVTTipi.vtMSSQL,
                                                          "MAHIR2021",
                                                          "TEMELSET",
                                                          "",
                                                          "netopenx",
                                                          "1234",
                                                          0);

                            fatura = kernel.yeniFatura(sirket, TFaturaTip.ftSFat);
                            fatUst = fatura.Ust();
                            fatUst.FATIRS_NO = invoice.ID.Value.Replace("2022", "000"); //fatura.YeniNumara("A");
                            fatUst.GIB_FATIRS_NO = invoice.ID.Value;
                            fatUst.CariKod = cariKod;
                            fatUst.Tarih = invoice.IssueDate.Value;
                            fatUst.ENTEGRE_TRH = invoice.IssueDate.Value;
                            fatUst.FiiliTarih = invoice.IssueDate.Value;
                            fatUst.SIPARIS_TEST = invoice.IssueDate.Value;
                            fatUst.FIYATTARIHI = invoice.IssueDate.Value;
                            fatUst.ODEMETARIHI = invoice.IssueDate.Value;
                            fatUst.ODEMEGUNU = 0;
                            fatUst.TIPI = TFaturaTipi.ft_Acik;
                            fatUst.Proje_Kodu = "00";
                            fatUst.KDV_DAHILMI = false;
                            fatUst.GEN_ISK1T = (double)invoice.AllowanceCharge[0].Amount.Value;
                            fatUst.GEN_ISK1O = (double)invoice.AllowanceCharge[0].MultiplierFactorNumeric.Value * 100;


                            //fatUst.BRUTTUTAR = 2;
                            //fatUst.KDV = 0;

                            for (int i = 0; i < invoice.InvoiceLine.Length; i++)
                            {
                                fatKalem = fatura.kalemYeni(invoice.InvoiceLine[i].Item.SellersItemIdentification.ID.Value);
                                fatKalem.DEPO_KODU = 1;
                                fatKalem.STra_GCMIK = (double)invoice.InvoiceLine[i].InvoicedQuantity.Value;
                                fatKalem.STra_NF = (double)invoice.InvoiceLine[i].Price.PriceAmount.Value;
                                fatKalem.STra_BF = (double)invoice.InvoiceLine[i].Price.PriceAmount.Value;
                                fatKalem.STra_SIPNUM = invoice.OrderReference.ID.Value;
                                //fatKalem.Irsaliyetar = invoice.OrderReference.IssueDate.Value;
                                if (invoice.InvoiceLine[0].AllowanceCharge.Any())
                                    fatKalem.STra_SatIsk = (double)invoice.InvoiceLine[0].AllowanceCharge[0].MultiplierFactorNumeric.Value * 100;


                                //fatKalem.Stra_IrsKont = invoice.InvoiceLine[i].OrderLineReference[0].LineI;
                                fatKalem.STra_KDV = (double)invoice.InvoiceLine[i].TaxTotal.TaxSubtotal[0].Percent.Value;
                                //fatKalem.SatisKDVOran = invoice.TaxTotal.TaxSubtotal.Percent;
                                if (invoice.TaxTotal[0].TaxSubtotal[0].TaxableAmount.currencyID == "USD")
                                {
                                    fatKalem.STra_DOVTIP = 1;
                                    fatKalem.STra_DOVFIAT = (double)invoice.TaxTotal[0].TaxSubtotal[0].TaxableAmount.Value;
                                    //fatKalem.STra_NF = result.
                                    //fatKalem.STra_BF = (double)result.InvoiceLine[i].AllowanceCharge.Amount.Value;
                                }
                                else
                                {

                                }
                            }

                            fatura.kayitYeni();

                            logwriter.WriteLine($"{xmlfile} kayıt başarılı");
                        }
                        catch (Exception exc1)
                        {
                            logwriter.WriteLine($"{xmlfile} hata {exc1.Message}");
                            MessageBox.Show(string.Format("Xml dosyasi acilirken hata:{0}{1}", Environment.NewLine, exc1), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(fatKalem);
                            Marshal.ReleaseComObject(fatUst);
                            Marshal.ReleaseComObject(fatura);
                            Marshal.ReleaseComObject(sirket);
                            kernel.FreeNetsisLibrary();
                            Marshal.ReleaseComObject(kernel);
                        }
                    }
                }

                /*using (FileStream fileStream = new FileStream(xmlfile, FileMode.Open))
                {
                    EFaturaAktarim.Envelop.Invoice result = (EFaturaAktarim.Envelop.Invoice)serializer.Deserialize(fileStream);
                    if (result != null)
                    {
                        string cariKod = "";
                        command.CommandText = "select c.CARI_KOD,VERGI_NUMARASI,ce.TCKIMLIKNO from tblcasabit as c inner join TBLCASABITEK as ce on c.CARI_KOD=ce.CARI_KOD where VERGI_NUMARASI = @TKN or ce.TCKIMLIKNO = @TKN";
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "TKN";
                        parameter.Value = result.AccountingSupplierParty.Party.PartyIdentification[0].ID.Value;
                        parameter.DbType = DbType.String;
                        command.Parameters.Add(parameter);
                        BaseRepository.FixCommand(command, false);
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (reader != null && reader.Read())
                            {
                                cariKod = reader.GetValue(0).ToString();
                            }
                        }
                        command.Parameters.Clear();

                        command.CommandText = string.Format("SELECT COUNT(*) FROM TBLFATUIRS WITH (NOLOCK) WHERE FTIRSIP = '1' AND CARI_KODU = '{0}' AND FATIRS_NO = '{1}'", cariKod, result.ID.Value);
                        var checkFat = command.ExecuteScalar();
                        if (checkFat != null && Convert.ToInt32(checkFat) > 0) return;

                        Kernel kernel = new Kernel();
                        Sirket sirket = default(Sirket);
                        Fatura fatura = default(Fatura);
                        FatUst fatUst = default(FatUst);
                        FatKalem fatKalem = default(FatKalem);

                        try
                        {
                            sirket = kernel.yeniSirket(TVTTipi.vtMSSQL,
                                                          "MAHIR2021",
                                                          "TEMELSET",
                                                          "",
                                                          "netopenx",
                                                          "1234",
                                                          0);
                            fatura = kernel.yeniFatura(sirket, TFaturaTip.ftSFat);
                            fatUst = fatura.Ust();
                            fatUst.FATIRS_NO = result.ID.Value.Replace("2022", "000"); //fatura.YeniNumara("A");
                            fatUst.GIB_FATIRS_NO = result.ID.Value;
                            fatUst.CariKod = cariKod;
                            fatUst.Tarih = result.IssueDate;
                            fatUst.ENTEGRE_TRH = result.IssueDate;
                            fatUst.FiiliTarih = result.IssueDate;
                            fatUst.SIPARIS_TEST = result.IssueDate;
                            fatUst.FIYATTARIHI = result.IssueDate;
                            fatUst.TIPI = TFaturaTipi.ft_Acik;
                            fatUst.Proje_Kodu = "00";
                            fatUst.KDV_DAHILMI = false;

                            //fatUst.BRUTTUTAR = 2;
                            //fatUst.KDV = 0;

                            for (int i = 0; i < result.InvoiceLine.Length; i++)
                            {
                                fatKalem = fatura.kalemYeni(result.InvoiceLine[i].Item.SellersItemIdentification.ID.Value);
                                fatKalem.DEPO_KODU = 1;
                                fatKalem.STra_GCMIK = (double)result.InvoiceLine[i].InvoicedQuantity.Value;
                                fatKalem.STra_NF = (double)result.InvoiceLine[i].AllowanceCharge.Amount.Value;
                                fatKalem.STra_BF = (double)result.InvoiceLine[i].AllowanceCharge.Amount.Value;
                                fatKalem.Irsaliyeno = result.OrderReference.ID.Value;
                                fatKalem.Irsaliyetar = result.OrderReference.IssueDate;
                                //fatKalem.DOVIZ_TURU = 1;
                                //fatKalem.DOVTIP = 1;


                                fatKalem.Stra_IrsKont = result.InvoiceLine[i].OrderLineReference.LineID;
                                fatKalem.STra_KDV = (double)result.InvoiceLine[i].TaxTotal.TaxAmount.Value;
                                fatKalem.SatisKDVOran = result.TaxTotal.TaxSubtotal.Percent;
                                if (result.TaxTotal.TaxSubtotal.TaxableAmount.currencyID == "USD")
                                {
                                    fatKalem.STra_DOVTIP = 1;
                                    fatKalem.STra_DOVFIAT = result.TaxTotal.TaxSubtotal.TaxableAmount.Value;
                                    //fatKalem.STra_NF = result.
                                    //fatKalem.STra_BF = (double)result.InvoiceLine[i].AllowanceCharge.Amount.Value;
                                }
                                else
                                {

                                }
                            }

                            fatura.kayitYeni();
                        }
                        catch (Exception exc1)
                        {
                            MessageBox.Show(string.Format("Xml dosyasi acilirken hata:{0}{1}", Environment.NewLine, exc1), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(fatKalem);
                            Marshal.ReleaseComObject(fatUst);
                            Marshal.ReleaseComObject(fatura);
                            Marshal.ReleaseComObject(sirket);
                            kernel.FreeNetsisLibrary();
                            Marshal.ReleaseComObject(kernel);
                        }

                        //var fatura = new ItemSlips();
                        //fatura.FatUst = new ItemSlipsHeader();
                        //fatura.FaturaTip = JTFaturaTip.ftSFat;
                        //fatura.FatUst.TIPI = JTFaturaTipi.ft_Bos;
                        //fatura.FatUst.Tarih = result.IssueDate;
                        //fatura.FatUst.FATIRS_NO = result.ID.Value;

                        //fatura.FatUst.CariKod = cariKod;

                        //fatura.FatUst.AMBHARTUR = JTAmbarHarTur.htDepolar;
                        //fatura.FatUst.Sube_Kodu = 0;
                        //fatura.FatUst.EKACK1 = string.Concat(result.Note);

                        //fatura.Kalems = new List<ItemSlipLines>();
                        //fatura.YedekKalems = new List<ItemSlipLines>();

                        //for (int i = 0; i < result.InvoiceLine.Length; i++)
                        //{
                        //    ItemSlipLines lines = new ItemSlipLines();
                        //    lines.StokKodu = result.InvoiceLine[i].Item.SellersItemIdentification.ID.Value;
                        //    lines.STra_GCMIK = (double)result.InvoiceLine[i].InvoicedQuantity.Value;
                        //    lines.STra_NF = (double)result.InvoiceLine[i].AllowanceCharge.Amount.Value;


                        //    fatura.Kalems.Add(lines);
                        //    fatura.YedekKalems.Add(lines);
                        //}

                        //var _oAuth2 = new oAuth2(textUrl.Text);
                        //_oAuth2.Login(GetLogin());
                        //ItemSlipsManager _manager = new ItemSlipsManager(_oAuth2);
                        //var restResult = _manager.PostInternal(fatura);
                        //if(restResult != null)
                        //{
                        //}
                    }
                }*/
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Xml dosyasi acilirken hata:{0}{1}", Environment.NewLine, exc), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            finally
            {
                try
                {
                    File.Delete(xmlfile);
                }
                catch
                {
                }
            }
        }

        private string GetCariKodFromTkn(string tkn)
        {
            string ckod = string.Empty;
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder["user id"] = this.txtSourceUser.Text;
            builder["password"] = this.txtSourcePassword.Text;
            builder["Data Source"] = this.txtSourceServer.Text;
            if (this.cmbSourceDbType.SelectedIndex == 0)
            {
                builder["initial catalog"] = this.txtSourceSchema.Text;
            }

            DbConnection connection;
            BaseRepository.Configure(builder.ConnectionString, (this.cmbSourceDbType.SelectedIndex == 0) ? string.Empty : this.txtSourceSchema.Text, (this.cmbSourceDbType.SelectedIndex == 0) ? ProviderType.SystemDataSqlClient : ProviderType.SystemDataOracleClient);
            using (connection = BaseRepository.CreateConnection())
            {
                connection.Open();
                DbCommand command = connection.CreateCommand();
                command.CommandText = "select c.CARI_KOD,VERGI_NUMARASI,ce.TCKIMLIKNO from tblcasabit as c inner join TBLCASABITEK as ce on c.CARI_KOD=ce.CARI_KOD where VERGI_NUMARASI = @TKN or ce.TCKIMLIKNO = @TKN";
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = "TKN";
                parameter.Value = tkn;
                parameter.DbType = DbType.String;
                command.Parameters.Add(parameter);
                BaseRepository.FixCommand(command, false);
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader != null && reader.Read())
                    {
                        ckod = reader.GetValue(0).ToString();
                    }
                }
            }
            return ckod;
        }

        private JLogin GetLogin()
        {
            return new JLogin()
            {
                BranchCode = 0,   //sube kodu bilgisi
                NetsisUser = "netopenx", //netsis kullanıcı adı bilgisi
                NetsisPassword = "1234", //netsis şifre bilgisi
                DbType = JNVTTipi.vtMSSQL, //veritabanı tipi
                DbName = "MAHIR2021", //şirket bilgisi
                DbPassword = "", //veritabanı şifre bilgisi
                DbUser = "TEMELSET" //veritabanı kullanıcı adı bilgisi
            };
        }

        private string GetPrivateProfile(string key, string dvalue)
        {
            var revinifile = string.Concat(Application.StartupPath, "\\config.ini");
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString("CONFIG", key, "", temp, 255, revinifile);
            if (i == 0)
            {
                WritePrivateProfileString("CONFIG", key, dvalue, revinifile);
                return dvalue;
            }
            return temp.ToString();
        }

        private void WritePrivateProfile(string key, string value)
        {
            var revinifile = string.Concat(Application.StartupPath, "\\config.ini");
            WritePrivateProfileString("CONFIG", key, value, revinifile);

        }

        private void SaveProfile()
        {
            WritePrivateProfile("cmbFolderDbType", cmbFolderDbType.SelectedIndex.ToString());

            WritePrivateProfile("txtFolderServer", txtFolderServer.Text);

            WritePrivateProfile("txtFolderSchema", txtFolderSchema.Text);

            WritePrivateProfile("txtFolderUser", txtFolderUser.Text);

            WritePrivateProfile("txtFolderPass", txtFolderPass.Text);

            WritePrivateProfile("txtFolderBranch", txtFolderBranch.Text);

            WritePrivateProfile("txtSelectedFolder", txtSelectedFolder.Text);

            WritePrivateProfile("textUrl", textUrl.Text);

            WritePrivateProfile("rdIn", rdIn.Checked ? "1" : "0");

            WritePrivateProfile("checkFatura", checkFatura.Checked ? "1" : "0");
        }
    }
}

