using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppCamera.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppCamera
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocalNotificationPage : ContentPage
    {
        public LocalNotificationPage(Medicamento medicamento)
        {
            InitializeComponent();

            BindingContext = new LocalNotificationPageViewModel(medicamento);
    

            txtNoti.Text = "Vas a generar un recordatorio para el medicamento: "+ medicamento.nombre;
            txtDosificacion.Text = "Recuerda que debes tomar: " + medicamento.dosificacion;
            string frecuencia = ""; //1 - 12  - 8 
            string tiempo = ""; //dia - horas - horas
            string[] palabras = new string[] { "" };
            palabras = medicamento.dosificacion.Split(' ');

            frecuencia = palabras[3];
            tiempo = palabras[4];

            if (String.Equals(tiempo,"HORAS"))
            {
                txtrRecomendacionHora.Text = "Tenga en cuenta que este medicamento se toma cada " + frecuencia + " HORAS";
            }
            if (String.Equals(tiempo, "DIAS"))
            {
                txtrRecomendacionHora.Text = "Tenga en cuenta que este medicamento se toma cada "+ frecuencia +" DIAS";
            }

            DisplayAlert("¡frecuen!", frecuencia, "OK");

            DisplayAlert("¡time!", tiempo, "OK");


        }
    }
}