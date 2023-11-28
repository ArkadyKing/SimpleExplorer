using System.Diagnostics;
using System.IO;

namespace SimpleExplorer
{
	public partial class Form1 : Form
	{
		private Stack<string> navigationHistory = new Stack<string>();
		private Stack<string> forwardHistory = new Stack<string>();
		public Form1()
		{
			InitializeComponent();
			InitializeFileBrowser();
		}

		private void InitializeFileBrowser()
		{
			foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				driveComboBox.Items.Add(drive.Name);
			}

			driveComboBox.SelectedIndexChanged += driveComboBox_SelectedIndexChanged;
			driveComboBox.SelectedIndex = 0;
			string initialD = driveComboBox.SelectedItem?.ToString();

			// Устанавливаем начальный каталог
			PopulateListView(initialD);

			listView.MouseDoubleClick += ListView_MouseDoubleClick;
		}

		private void PopulateListView(string directory)
		{
			// Очищаем элементы
			listView.Items.Clear();

			try
			{
                Environment.CurrentDirectory = directory;
                pathTextBox.Text = directory;
                // Получаем папки
                string[] directories = Directory.GetDirectories(directory);
				ListViewItem.ListViewSubItem[] subItems;
				foreach (string dir in directories)
				{
					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					ListViewItem item = new ListViewItem(dirInfo.Name, "folder");
					subItems = new ListViewItem.ListViewSubItem[]
						{new ListViewItem.ListViewSubItem(item, "Directory"),
						new ListViewItem.ListViewSubItem(item,
						dirInfo.LastAccessTime.ToShortDateString())};
					item.SubItems.AddRange(subItems);
					listView.Items.Add(item);
				}

				// Получаем файлы
				string[] files = Directory.GetFiles(directory);
				foreach (string file in files)
				{
					FileInfo fileInfo = new FileInfo(file);
					ListViewItem item = new ListViewItem(fileInfo.Name, "file");
					subItems = new ListViewItem.ListViewSubItem[]
						{ new ListViewItem.ListViewSubItem(item, "File"),
						new ListViewItem.ListViewSubItem(item,
						fileInfo.LastAccessTime.ToShortDateString())};

					item.SubItems.AddRange(subItems);
					listView.Items.Add(item);
				}
				navigationHistory.Push(directory);
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
			catch (Exception ex)
			{
				MessageBox.Show($"Error accessing directory: {ex.Message}");
			}

		}

		private void driveComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedDrive = driveComboBox.SelectedItem?.ToString();
			PopulateListView(selectedDrive);
		}

		private void buttonUp_Click(object sender, EventArgs e)
		{
			string currentDirectory = Environment.CurrentDirectory;

			// Получаем родительский каталог
			DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);

			if (parentDirectory != null)
			{
				// Заполняем ListView содержимым родительского каталога.
				PopulateListView(parentDirectory.FullName);
			}
		}

		private void listView_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listView.SelectedItems.Count > 0)
			{
				string selectedItemText = listView.SelectedItems[0].Text;
				string selectedPath = Path.Combine(Environment.CurrentDirectory, selectedItemText);

				if (Directory.Exists(selectedPath))
				{
					// Если папка, то переходим в неё
					PopulateListView(selectedPath);
				}
				else
				{
					try
					{
						// Открыть выбранный файл
						var p = new Process();
						p.StartInfo = new ProcessStartInfo(selectedPath)
						{
							UseShellExecute = true
						};
						p.Start();
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error opening file: {ex.Message}");
					}
				}
			}
		}

		private void buttonBack_Click(object sender, EventArgs e)
		{
			if (navigationHistory.Count > 0)
			{
				forwardHistory.Push(Environment.CurrentDirectory);
				navigationHistory.Pop();
                string previousDirectory = navigationHistory.Pop();
				PopulateListView(previousDirectory);
			}
		}

		private void buttonForward_Click(object sender, EventArgs e)
		{
			if (forwardHistory.Count > 0)
			{
				navigationHistory.Push(Environment.CurrentDirectory);
				string nextDirectory = forwardHistory.Pop();
				PopulateListView(nextDirectory);
			}
		}
	}
}