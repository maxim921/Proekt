using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace proekt
{
    public partial class MainWindow : Window
    {
        FleetDbEntities db = new FleetDbEntities();
        Vehicles selCar;
        Drivers selDrv;

        public MainWindow()
        {
            InitializeComponent();
            LoadAll();
        }

        void LoadAll()
        {
            CarsGrid.ItemsSource = db.Vehicles.ToList();
            DriversGrid.ItemsSource = db.Drivers.ToList();
            TripsGrid.ItemsSource = db.Routes.Include("Vehicles").Include("Drivers").ToList();

            cbVeh.ItemsSource = db.Vehicles.ToList();
            cbVeh.DisplayMemberPath = "Model";
            cbDrv.ItemsSource = db.Drivers.ToList();
            cbDrv.DisplayMemberPath = "FullName";
            cbFVeh.ItemsSource = db.Vehicles.ToList();
            cbFVeh.DisplayMemberPath = "Model";
            cbFDrv.ItemsSource = db.Drivers.ToList();
            cbFDrv.DisplayMemberPath = "FullName";
        }

        void CarsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selCar = (Vehicles)CarsGrid.SelectedItem;
            if (selCar == null) return;
            tbPlate.Text = selCar.LicensePlate;
            tbModel.Text = selCar.Model;
            tbManuf.Text = selCar.Manufacturer;
            tbYear.Text = selCar.YearOfManufacture.ToString();
            tbMileage.Text = selCar.Mileage.ToString();
            cbStatus.SelectedIndex = selCar.Status == "В ремонте" ? 1 : 0;
        }

        bool CheckCar()
        {
            if (!Regex.IsMatch(tbPlate.Text.Trim(), @"^[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$"))
            { MessageBox.Show("Неверно введён гос. номер (пример: А123ВС77)"); return false; }
            if (tbModel.Text.Trim() == "") { MessageBox.Show("Введите модель"); return false; }
            int y;
            if (!int.TryParse(tbYear.Text, out y) || y < 1980 || y > DateTime.Now.Year)
            { MessageBox.Show("Введите корректный год выпуска"); return false; }
            double m;
            if (!double.TryParse(tbMileage.Text, out m) || m < 0)
            { MessageBox.Show("Введите корректный пробег"); return false; }
            return true;
        }

        void BtnAddCar_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCar()) return;
            string plate = tbPlate.Text.Trim().ToUpper();
            if (db.Vehicles.Any(v => v.LicensePlate == plate))
            { MessageBox.Show("Машина с таким номером уже есть"); return; }

            db.Vehicles.Add(new Vehicles
            {
                LicensePlate = plate,
                Model = tbModel.Text.Trim(),
                Manufacturer = tbManuf.Text.Trim(),
                YearOfManufacture = int.Parse(tbYear.Text),
                Mileage = double.Parse(tbMileage.Text),
                Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
            });
            db.SaveChanges();
            LoadAll();
        }

        void BtnEditCar_Click(object sender, RoutedEventArgs e)
        {
            if (selCar == null) { MessageBox.Show("Выберите машину в таблице"); return; }
            if (!CheckCar()) return;
            selCar.LicensePlate = tbPlate.Text.Trim().ToUpper();
            selCar.Model = tbModel.Text.Trim();
            selCar.Manufacturer = tbManuf.Text.Trim();
            selCar.YearOfManufacture = int.Parse(tbYear.Text);
            selCar.Mileage = double.Parse(tbMileage.Text);
            selCar.Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString();
            db.SaveChanges();
            LoadAll();
        }

        void BtnDeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (selCar == null) { MessageBox.Show("Выберите машину в таблице"); return; }
            db.Routes.RemoveRange(db.Routes.Where(r => r.VehicleId == selCar.Id));
            db.Vehicles.Remove(selCar);
            db.SaveChanges();
            LoadAll();
        }

        void BtnSortCars_Click(object sender, RoutedEventArgs e)
        {
            CarsGrid.ItemsSource = db.Vehicles.OrderBy(v => v.Mileage).ToList();
        }

        void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            var lines = db.Vehicles
                .OrderByDescending(v => v.Mileage)
                .Select(v => v.LicensePlate + " " + v.Model + " — " + v.Mileage + " км");
            MessageBox.Show(string.Join("\n", lines), "Суммарный пробег машин");
        }

        void DriversGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selDrv = (Drivers)DriversGrid.SelectedItem;
            if (selDrv == null) return;
            tbFio.Text = selDrv.FullName;
            tbLicense.Text = selDrv.LicenseNumber;
            tbPhone.Text = selDrv.PhoneNumber;
            tbExp.Text = selDrv.Experience.ToString();
            tbEmail.Text = selDrv.Email;
            pbPass.Password = selDrv.Password;
        }

        bool CheckDriver()
        {
            if (tbFio.Text.Trim() == "") { MessageBox.Show("Введите ФИО"); return false; }
            if (tbLicense.Text.Trim() == "") { MessageBox.Show("Введите номер прав"); return false; }
            int exp;
            if (!int.TryParse(tbExp.Text, out exp) || exp < 0) { MessageBox.Show("Введите стаж числом"); return false; }
            return true;
        }

        void BtnAddDriver_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDriver()) return;
            db.Drivers.Add(new Drivers
            {
                FullName = tbFio.Text.Trim(),
                LicenseNumber = tbLicense.Text.Trim(),
                PhoneNumber = tbPhone.Text.Trim(),
                Experience = int.Parse(tbExp.Text),
                Email = tbEmail.Text.Trim(),
                Password = pbPass.Password
            });
            db.SaveChanges();
            LoadAll();
        }

        void BtnEditDriver_Click(object sender, RoutedEventArgs e)
        {
            if (selDrv == null) { MessageBox.Show("Выберите водителя в таблице"); return; }
            if (!CheckDriver()) return;
            selDrv.FullName = tbFio.Text.Trim();
            selDrv.LicenseNumber = tbLicense.Text.Trim();
            selDrv.PhoneNumber = tbPhone.Text.Trim();
            selDrv.Experience = int.Parse(tbExp.Text);
            selDrv.Email = tbEmail.Text.Trim();
            selDrv.Password = pbPass.Password;
            db.SaveChanges();
            LoadAll();
        }

        void BtnDeleteDriver_Click(object sender, RoutedEventArgs e)
        {
            if (selDrv == null) { MessageBox.Show("Выберите водителя в таблице"); return; }
            db.Routes.RemoveRange(db.Routes.Where(r => r.DriverId == selDrv.Id));
            db.Drivers.Remove(selDrv);
            db.SaveChanges();
            LoadAll();
        }

        void BtnRegisterTrip_Click(object sender, RoutedEventArgs e)
        {
            var veh = (Vehicles)cbVeh.SelectedItem;
            var drv = (Drivers)cbDrv.SelectedItem;
            if (veh == null || drv == null) { MessageBox.Show("Выберите машину и водителя"); return; }
            double dist;
            if (!double.TryParse(tbDist.Text, out dist) || dist <= 0)
            { MessageBox.Show("Введите расстояние числом"); return; }
            if (veh.Status == "В рейсе") { MessageBox.Show("Эта машина уже в рейсе"); return; }

            db.Routes.Add(new Routes
            {
                VehicleId = veh.Id,
                DriverId = drv.Id,
                StartDate = dpStart.SelectedDate ?? DateTime.Today,
                StartLocation = tbStartLoc.Text.Trim(),
                EndLocation = tbEndLoc.Text.Trim(),
                Distance = dist
            });
            veh.Status = "В рейсе";
            db.SaveChanges();
            LoadAll();
        }

        void BtnEndTrip_Click(object sender, RoutedEventArgs e)
        {
            var trip = (Routes)TripsGrid.SelectedItem;
            if (trip == null) { MessageBox.Show("Выберите рейс в таблице"); return; }
            if (trip.EndDate != null) { MessageBox.Show("Рейс уже завершён"); return; }

            trip.EndDate = DateTime.Today;
            var veh = db.Vehicles.Find(trip.VehicleId);
            veh.Status = "Доступен";
            veh.Mileage += trip.Distance;
            db.SaveChanges();
            LoadAll();
        }

        void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            var query = db.Routes.Include("Vehicles").Include("Drivers").AsQueryable();

            if (dpFrom.SelectedDate != null)
                query = query.Where(r => r.StartDate >= dpFrom.SelectedDate);
            if (dpTo.SelectedDate != null)
                query = query.Where(r => r.StartDate <= dpTo.SelectedDate);
            if (cbFVeh.SelectedItem != null)
                query = query.Where(r => r.VehicleId == ((Vehicles)cbFVeh.SelectedItem).Id);
            if (cbFDrv.SelectedItem != null)
                query = query.Where(r => r.DriverId == ((Drivers)cbFDrv.SelectedItem).Id);

            TripsGrid.ItemsSource = query.ToList();
        }

        void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            cbFVeh.SelectedItem = null;
            cbFDrv.SelectedItem = null;
            LoadAll();
        }

        void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (selCar == null) { MessageBox.Show("Сначала выберите машину на вкладке «Автомобили»"); return; }
            TripsGrid.ItemsSource = db.Routes.Include("Vehicles").Include("Drivers")
                .Where(r => r.VehicleId == selCar.Id)
                .OrderByDescending(r => r.StartDate)
                .ToList();
        }

        void BtnHistoryDrv_Click(object sender, RoutedEventArgs e)
        {
            if (selDrv == null) { MessageBox.Show("Сначала выберите водителя на вкладке «Водители»"); return; }
            TripsGrid.ItemsSource = db.Routes.Include("Vehicles").Include("Drivers")
                .Where(r => r.DriverId == selDrv.Id)
                .OrderByDescending(r => r.StartDate)
                .ToList();
        }
    }
}
