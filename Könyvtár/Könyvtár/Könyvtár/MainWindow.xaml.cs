using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace Könyvtár
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Portot manuálisan át kell állítani                      ----
        private string _connectionString = "server=localhost;port=3306;uid=root;pwd=;Convert Zero Datetime=True;database=konyvtar";
        private string _DBName = "konyvtar";
        private MySqlConnection _DbConnection;
        private Database DB;
        private string SelectedTableName;
        private List<TextBox> ExistingTBs;
        private List<string> DBs;

        public MainWindow()
        {
            InitializeComponent();

            //Get all databases
            _DbConnection = new MySqlConnection(_connectionString);
            _DbConnection.Open();
            var cmd = _DbConnection.CreateCommand();
            cmd.CommandText = $"SHOW DATABASES";
            var reader = cmd.ExecuteReader();
            DBs = new List<string>();
            while (reader.Read())
            {
                DBs.Add(reader.GetString(0));
            }
            foreach (var LBDB in DBs)
            {
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = LBDB;
                DataBasesLB.Items.Add(LBI);
            }
            reader.Close();
            _DbConnection.Close();

            OpenDB();
        }

        public void ClearAll() 
        {
            DataBasesLB.Visibility = Visibility.Collapsed;
            AddBtn.IsEnabled = true;
            RemoveBtn.IsEnabled = true;
            ModifyBtn.IsEnabled = true;
            DetailsGrid.Children.Clear();
            DataListBox.Items.Clear();
            //MainMenuItem.Items.Clear();
            DynamicMenuItem.Items.Clear();
            DetailsGrid.Visibility = Visibility.Visible;
            AddGrid.Children.Clear();
            AddGrid.Visibility = Visibility.Collapsed;
            AddOkBtn.Visibility = Visibility.Collapsed;
            ModifyBtn.Visibility = Visibility.Visible;
            ModifyOkBtn.Visibility = Visibility.Collapsed;
            CancelBtn.Visibility = Visibility.Collapsed;
            RemoveBtn.Visibility = Visibility.Visible;
            AddBtn.Visibility = Visibility.Visible;
            DataListBox.IsEnabled = true;
            DynamicMenu.IsEnabled = true;
            MainMenu.IsEnabled = true;
            DataListBox.Visibility = Visibility.Visible;
        }
        public void OpenDB()
        {
            ClearAll();
            DB = null;
            DB = new Database(_DBName);
            _connectionString = DB.connection;
            _DbConnection = new MySqlConnection(_connectionString);
            _DbConnection.Open();
            ExistingTBs = new List<TextBox>();
            GetTables();
            try 
            {
                SelectedTableName = DB.TableNames[0];
            }
            catch 
            {
                MessageBox.Show("Üres, vagy hibás adatbázis");
                _DBName = "konyvtar";
                OpenDB();
            }
                

            Render(0, DB.Tables[SelectedTableName].header.Count, DB.Tables[SelectedTableName],DetailsGrid,false);


            var cmd = _DbConnection.CreateCommand();
            cmd.CommandText = $"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_name = '{SelectedTableName}' AND column_key = 'PRI'";
            var reader = cmd.ExecuteReader();
            string temp = "";
            while (reader.Read())
            {
                temp = reader.GetString(0);
            }
            cmd = _DbConnection.CreateCommand();
            cmd.CommandText = $"SELECT {temp} FROM {SelectedTableName}";
            reader.Close();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = reader.GetString(0);
                DataListBox.Items.Add(LBI);
            }
            reader.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GetTables() 
        {
            try
            {
                //List<Database> tempDBs = new List<Database>();


                var cmd = _DbConnection.CreateCommand();
                cmd.CommandText = $"SELECT TABLE_NAME FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema = '{_DBName}'";

                var reader = cmd.ExecuteReader();
                List<string> temp = new List<string>();
                while (reader.Read())
                {
                    //MessageBox.Show($"{reader.GetString(0)}");
                    temp.Add(reader.GetString(0));
                }
                DB.TableNames = temp;

                for (int i = 0; i < DB.TableNames.Count(); i++)
                {
                    MenuItem MI = new MenuItem();
                    MI.Header = DB.TableNames[i];
                    this.DynamicMenuItem.Items.Add(MI);
                    MI.BorderBrush = Brushes.Purple;
                    MI.Background = ColorConverter.GetColorFromHexa("#FF060606");
                    MI.Foreground = Brushes.White;
                    MI.Click += Table_Click;
                    //MI.
                }
                foreach (var item in DB.TableNames)
                {
                    DB.Tables[item] = new Table { header = new List<string>(), Rows = new List<Dictionary<string, string>>() };
                }
                foreach (var item in DB.TableNames)
                {
                    //var tempTableList = new List< Dictionary<string, Table>>();
                    var tempHeader = new List<string>();
                    var tempRows = new List<Dictionary<string, string>>();

                    cmd = _DbConnection.CreateCommand();
                    cmd.CommandText = $"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_name = '{item}' AND table_schema = '{_DBName}'";
                    reader.Close();
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        tempHeader.Add(reader.GetString(0));
                    }

                    cmd = _DbConnection.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM {item}";
                    reader.Close();
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var tempD = new Dictionary<string, string>();
                        for (int i = 0; i < tempHeader.Count(); i++)
                        {
                            //MessageBox.Show(reader.GetFieldType(i).ToString());


                            tempD[tempHeader[i]] = reader.GetString(i);

                        }
                        tempRows.Add(tempD);

                        var tempTable = new Table();
                        tempTable.header = tempHeader;
                        tempTable.Rows = tempRows;
                        DB.Tables[item] = tempTable;
                    }
                    reader.Close();
                }
            }
            catch 
            {
                MessageBox.Show("Nem sikerült betölteni ezt az adatbázist");
                _DBName = "konyvtar";
                OpenDB();
            }
        }

        private void Render(int RowNumber,int Amount, Table Table,Grid GridName, bool IsEnabledV) 
        {
            GridName.Children.Clear();
            ExistingTBs.Clear();
            //MessageBox.Show("Generating...");


            for (int i = 0; i < Amount; i++)
            {
                var RowDfn = new RowDefinition();
                RowDfn.Height = GridLength.Auto;
                GridName.RowDefinitions.Add(RowDfn);
            }

            for (int i = 0; i < Amount; i++)
            {
                Label FrstLabel = new Label();
                FrstLabel.Content = $"{Table.header[i]}";
                FrstLabel.Name = $"a{i}1st";
                Grid.SetRow(FrstLabel, i);
                GridName.Children.Add(FrstLabel);

                TextBox ScndLabel = new TextBox();

                
                ScndLabel.Text = $"{Table.Rows[RowNumber][Table.header[i]]}";
                ScndLabel.Name = $"b{i}2nd";
                ScndLabel.IsEnabled = IsEnabledV;
                Grid.SetColumn(ScndLabel, 1);
                Grid.SetRow(ScndLabel, i);
                GridName.Children.Add(ScndLabel);
                ExistingTBs.Add(ScndLabel);
            }
        }

        private void DataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Table selected = DB.Tables[SelectedTableName];
            //MessageBox.Show(SelectedTableName);
            //MessageBox.Show(DB.Tables[SelectedTableName].header[0]);
            if(DataListBox.SelectedIndex != -1) 
            {
                Render(DataListBox.SelectedIndex, selected.header.Count(), selected, DetailsGrid,false);
            } 
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            MenuItem tempItem = (MenuItem)sender;
            DataListBox.Items.Clear();
            SelectedTableName = $"{tempItem.Header}";

            var cmd = _DbConnection.CreateCommand();
            cmd.CommandText = $"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_name = '{tempItem.Header}' AND column_key = 'PRI'";
            var reader = cmd.ExecuteReader();
            string temp = "";
            while (reader.Read())
            {
                temp = reader.GetString(0); 
            }

            //MessageBox.Show(temp);

            reader.Close();
            cmd = _DbConnection.CreateCommand();
            cmd.CommandText = $"SELECT {temp} FROM {tempItem.Header}";
            reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                ListBoxItem LBI = new ListBoxItem();
                LBI.Content = reader.GetString(0);
                DataListBox.Items.Add(LBI);
            }
            reader.Close();
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Table SelectedTable = DB.Tables[SelectedTableName];
                var cmd = _DbConnection.CreateCommand();
                bool IsFirst = true;

                cmd.CommandText = $"DELETE FROM {SelectedTableName} WHERE ";
                for (int i = 0; i < SelectedTable.header.Count; i++)
                {
                    if (!IsFirst)
                    {
                        cmd.CommandText += " AND ";
                    }
                    else
                    {
                        IsFirst = false;
                    }
                    cmd.CommandText += $"{SelectedTable.header[i]} = \"{SelectedTable.Rows[DataListBox.SelectedIndex][SelectedTable.header[i]]}\"";
                }
                var reader = cmd.ExecuteNonQuery();
                OpenDB();
                MessageBox.Show("Removed");
            }
            catch 
            {
                MessageBox.Show("Nincs kiválasztott mező");
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //MessageBox.Show(SelectedTableName);
                Table selected = DB.Tables[SelectedTableName];
                Table temp = new Table();
                Dictionary<string, string> tempRow = new Dictionary<string, string>();
                for (int i = 0; i < selected.header.Count; i++)
                {
                    tempRow[selected.header[i]] = "Type Here...";
                }
                temp.Rows = new List<Dictionary<string, string>>();
                temp.header = selected.header;
                temp.Rows.Add(tempRow);

                DataListBox.Items.Clear();
                Render(0, selected.header.Count(), temp, AddGrid,true);
                DetailsGrid.Visibility = Visibility.Collapsed;
                AddGrid.Visibility = Visibility.Visible;

                AddBtn.Visibility = Visibility.Collapsed;
                ModifyBtn.Visibility = Visibility.Collapsed;
                RemoveBtn.Visibility = Visibility.Collapsed;
                AddOkBtn.Visibility = Visibility.Visible;
                MainMenu.IsEnabled = false;
                DynamicMenu.IsEnabled = false;
            }
            catch 
            {
                MessageBox.Show("Nincs kiválasztott tábla");
            }
        }

        private void AOkBtn_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                List<string> types = new List<string>();
                var cmd = _DbConnection.CreateCommand();
                cmd.CommandText = $"SELECT DATA_TYPE FROM information_schema.columns WHERE table_name = '{SelectedTableName}' AND table_schema = '{_DBName}'";
                
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    types.Add(reader.GetString(0));
                    //MessageBox.Show(reader.GetString(0));
                }
                reader.Close();




                Table SelectedTable = DB.Tables[SelectedTableName];
                 cmd = _DbConnection.CreateCommand();
                bool IsFirst = true;

                cmd.CommandText = $"INSERT INTO {SelectedTableName}(";
                for (int i = 0; i < SelectedTable.header.Count; i++) 
                {
                if (!IsFirst)
                {
                    cmd.CommandText += ",";
                }
                else
                {
                    IsFirst = false;
                }
                cmd.CommandText += $"{SelectedTable.header[i]}";   
                }
                IsFirst = true;

                //cmd.CommandText += $" VALUES({ExistingTBs[0].Text},\"{ExistingTBs[1].Text}\",\"{ExistingTBs[2].Text}\"";
                //MessageBox.Show(cmd.CommandText);
                //cmd.ExecuteNonQuery();
                
                cmd.CommandText += ") VALUES(";

                for (int i = 0; i < SelectedTable.header.Count; i++)
                {
                    if (!IsFirst)
                    {
                        cmd.CommandText += ",";
                    }
                    else
                    {
                        IsFirst = false;
                    }
                    TextBox tempTB = ExistingTBs[i];
                    if (types[i] == "int") 
                    {
                        cmd.CommandText += $"{tempTB.Text}";
                    }
                    else
                    {
                        cmd.CommandText += $"\"{tempTB.Text}\"";
                    } 
                }
                cmd.CommandText += ")";
                //MessageBox.Show(cmd.CommandText);
                var RowCount = cmd.ExecuteNonQuery();
                MessageBox.Show("A hozzáadás sikeres");
            }
            catch 
            {
                MessageBox.Show("A hozzáadás sikertelen");
            }
            OpenDB();
            
        }

        private void ModifyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataListBox.SelectedIndex != -1)
                {
                    DynamicMenu.IsEnabled = false;
                    MainMenu.IsEnabled = false;
                    DataListBox.IsEnabled = false;
                    AddBtn.Visibility = Visibility.Collapsed;
                    ModifyBtn.Visibility = Visibility.Collapsed;
                    RemoveBtn.Visibility = Visibility.Collapsed;
                    ModifyOkBtn.Visibility = Visibility.Visible;
                    CancelBtn.Visibility = Visibility.Visible;

                    for (int i = 0; i < ExistingTBs.Count; i++)
                    {
                        ExistingTBs[i].IsEnabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Nincs kiválasztott elem");
                }
            }
            catch 
            {
                MessageBox.Show("Nincs kiválasztott elem");
                OpenDB();
            }
            
        }
        private void MOkBtn_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                bool empty = false;

                for (int i = 0; i < ExistingTBs.Count; i++)
                {
                    if (ExistingTBs[i].Text == "")
                    {
                        empty = true;
                    }
                }
                if (!empty)
                {
                    DataListBox.IsEnabled = true;
                    AddBtn.Visibility = Visibility.Visible;
                    ModifyBtn.Visibility = Visibility.Visible;
                    RemoveBtn.Visibility = Visibility.Visible;
                    ModifyOkBtn.Visibility = Visibility.Collapsed;
                    CancelBtn.Visibility = Visibility.Collapsed;

                    List<string> types = new List<string>();
                    Table SelectedTable = DB.Tables[SelectedTableName];
                    var cmd = _DbConnection.CreateCommand();
                    cmd.CommandText = $"SELECT DATA_TYPE FROM information_schema.columns WHERE table_name = '{SelectedTableName}' AND table_schema = '{_DBName}'";

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        types.Add(reader.GetString(0));
                        //MessageBox.Show(reader.GetString(0));
                    }
                    reader.Close();

                    bool IsFirst = true;
                    cmd = _DbConnection.CreateCommand();
                    cmd.CommandText = $"UPDATE {SelectedTableName} SET ";
                    for (int i = 0; i < ExistingTBs.Count; i++)
                    {
                        if (!IsFirst)
                        {
                            cmd.CommandText += ",";
                        }
                        else
                        {
                            IsFirst = false;
                        }
                        if (types[i] == "int")
                        {
                            cmd.CommandText += $"{SelectedTable.header[i]} = {ExistingTBs[i].Text}";
                        }
                        else
                        {
                            cmd.CommandText += $"{SelectedTable.header[i]} = \"{ExistingTBs[i].Text}\"";
                        }
                    }
                    cmd.CommandText += " WHERE ";
                    IsFirst = true;
                    for (int i = 0; i < ExistingTBs.Count; i++)
                    {
                        if (!IsFirst)
                        {
                            cmd.CommandText += " AND ";
                        }
                        else
                        {
                            IsFirst = false;
                        }
                        cmd.CommandText += $"{SelectedTable.header[i]} = \"{SelectedTable.Rows[DataListBox.SelectedIndex][SelectedTable.header[i]]}\"";
                    }
                    //MessageBox.Show(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                    OpenDB();
                }
                else
                {
                    MessageBox.Show("Nem lehet üres mező");
                }
            }
            catch 
            {
                MessageBox.Show("Az adatok hibásan lettek megadva");
                OpenDB();
            }
        }
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Változtatások elvetve");
            OpenDB();
        }

        private void SelectDB_Click(object sender, RoutedEventArgs e)
        {
            DynamicMenu.IsEnabled = false;
            DetailsGrid.Children.Clear();
            AddBtn.IsEnabled = false;
            ModifyBtn.IsEnabled = false;
            RemoveBtn.IsEnabled = false;
            DataListBox.Visibility = Visibility.Collapsed;
            DataBasesLB.Visibility = Visibility.Visible;

            
        }
        private void DataBasesLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DbConnection.Close();
            _DBName = DBs[DataBasesLB.SelectedIndex];
            OpenDB();
        }
    }
    public class ColorConverter
    {
        public static SolidColorBrush GetColorFromHexa(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                    Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16),
                    Convert.ToByte(hexaColor.Substring(7, 2), 16)
                )
            );
        }
    }
}
