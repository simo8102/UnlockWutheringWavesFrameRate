using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Newtonsoft.Json;

public class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new LauncherForm());
    }
}

public class LauncherForm : Form
{
    private Button selectButton;

    public LauncherForm()
    {
        Text = "一键解锁鸣潮帧率上限至120";
        Size = new System.Drawing.Size(400, 225);
        Icon = new Icon("appicon.ico"); // 指定图标文件路径
        selectButton = new Button
        {
            Text = "请选择鸣潮启动器",
            Location = new System.Drawing.Point(50, 50)
        };
        selectButton.Click += SelectButton_Click;

        Controls.Add(selectButton);
    }

    private void SelectButton_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            openFileDialog.Title = "请选择鸣潮启动器";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = openFileDialog.FileName;
                string base_dir = Path.GetDirectoryName(file_path);
                string local_storage_path = Path.Combine(base_dir, "Wuthering Waves Game", "Client", "Saved", "LocalStorage", "LocalStorage.db");

                if (File.Exists(local_storage_path))
                {
                    ModifyFrameRate(local_storage_path);
                }
                else
                {
                    MessageBox.Show("找不到文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void ModifyFrameRate(string db_path)
    {
        try
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={db_path};Version=3;"))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT value FROM LocalStorage WHERE key = 'GameQualitySetting'", conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string json = reader.GetString(0);
                        dynamic settings = JsonConvert.DeserializeObject(json);
                        settings["KeyCustomFrameRate"] = 120;
                        string new_value = JsonConvert.SerializeObject(settings);

                        using (SQLiteCommand updateCmd = new SQLiteCommand("UPDATE LocalStorage SET value = @newValue WHERE key = 'GameQualitySetting'", conn))
                        {
                            updateCmd.Parameters.AddWithValue("@newValue", new_value);
                            updateCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("帧率上限已设置为1200", "修改完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("找不到设置项！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
