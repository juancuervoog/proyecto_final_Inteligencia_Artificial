using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using AppCamera.DependencyServices;
using Xamarin.Forms;
namespace AppCamera.ViewModels
{
    public class LocalNotificationPageViewModel : INotifyPropertyChanged
    {
        Command _saveCommand;
        public Command SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                SetProperty(ref _saveCommand, value);
            }
        }
        bool _notificationONOFF;
        public bool NotificationONOFF
        {
            get
            {
                return _notificationONOFF;
            }
            set
            {
                SetProperty(ref _notificationONOFF, value);
                Switch_Toggled();
            }
        }
        void Switch_Toggled()
        {
            if (NotificationONOFF == false) //no esta seleccionado
            {
                MessageText = string.Empty;
                SelectedTime = DateTime.Now.TimeOfDay;
                SelectedDate = DateTime.Today;
                DependencyService.Get<ILocalNotificationService>().Cancel(0);
            }
        }
        DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                SetProperty(ref _selectedDate, value);
            }
        }
        TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
        public TimeSpan SelectedTime
        {
            get
            {
                return _selectedTime;
            }
            set
            {
                SetProperty(ref _selectedTime, value);
            }
        }
        string _messageText;
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                SetProperty(ref _messageText, value);
            }
        }
        public LocalNotificationPageViewModel(Medicamento medicamento)
        {
            SaveCommand = new Command(() => SaveLocalNotification(medicamento));
        }
        void SaveLocalNotification(Medicamento medicamento)
        {

            var date = (DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Day.ToString("00") + "-" + DateTime.Today.Year.ToString());
            var time = Convert.ToDateTime(SelectedTime.ToString()).ToString("HH:mm");
            var dateTime = date + " " + time;
            var selectedDateTime = DateTime.ParseExact(dateTime, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture);

            var nombreMedicamento = medicamento.nombre;
            string frecuencia = ""; //1 - 12  - 8 
            string tiempo = ""; //dia - horas - horas
            string[] palabras = new string[] { "" };
            palabras = medicamento.dosificacion.Split(' ');

            frecuencia = palabras[3];
            tiempo = palabras[4];

            if (!string.IsNullOrEmpty(nombreMedicamento))
            {
                DependencyService.Get<ILocalNotificationService>().Cancel(0);
                DependencyService.Get<ILocalNotificationService>().LocalNotification("Recuerda tomar tu medicamento", nombreMedicamento, 0, selectedDateTime, frecuencia, tiempo);
                App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Notification details saved successfully ", "Ok");
            }
            else
            {
                App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Please enter meassage", "OK");
            }

        }
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;
            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}