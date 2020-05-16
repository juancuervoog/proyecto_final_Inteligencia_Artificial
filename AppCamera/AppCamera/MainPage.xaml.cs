using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AppCamera
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private const string subscriptionKey = "d2ec2af794914d178210af3a64e021be";
        private byte[] imageArray = null;
        private IList<Medicamento> listaMedicamentos = new List<Medicamento>();
        public MainPage()
        {
            InitializeComponent();

            CameraButton.Clicked += CameraButton_Clicked;
   
        }
        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });

            if (photo != null)
                PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });

           

            using (MemoryStream memory = new MemoryStream()) //transformar imagen a un array de bytes
            {

                Stream stream = photo.GetStream();
                stream.CopyTo(memory);
                imageArray = memory.ToArray();
            }
        }

        private async void TagButton_Clicked(object sender, EventArgs e)
        {
            if (imageArray != null)
            {
                hacerAnalisis(imageArray);
            }
            else //en caso que no haya tomado ninguna foto
            {
                await DisplayAlert("¡ALERTA!", "Tienes que tomar primero una foto", "OK"); 
            }
        }

        public async void hacerAnalisis(byte[] byteData)
        {

            // RECONOCIMIENTO OCR
            /*
            //request parameters
            queryString["language"] = "es";
            queryString["detectOrientation"] = "true";
            var uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr?" + queryString;
           */

            //----------------------------------------RECOGNIZE TEXT-----------------------------------
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);


            //RECONOCIMIENTO Recognize Text
            //request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            //request parameters
            queryString["mode"] = "Printed";
            var uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/recognizeText?" + queryString;


            HttpResponseMessage response;

            //request body
        

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                mensaje.Text = "Estamos calculando, espera un momento por favor...";

                response = await client.PostAsync(uri, content);
            }


            if (response.StatusCode == System.Net.HttpStatusCode.Accepted) //lee la fotogafia
            {
                mensaje.Text = "HECHO!!!";

                string strUrl = response.Headers.GetValues("Operation-Location").First();

                response = await client.GetAsync(strUrl);

                bool seguir = true;

                while (seguir)
                {
                    response = await client.GetAsync(strUrl);

                    string content = await response.Content.ReadAsStringAsync();

                    if (content.Contains("Succeeded"))
                    {
                        seguir = false;
                    }
                }
            }

            //------------------------------fin recognize text--------------------------------------------

            //get the JSON response
            string contentString = await response.Content.ReadAsStringAsync();

            


            if (contentString != null)
            {
                listaMedicamentos = trabajarJson(contentString);
            }

            //mensaje.Text = contentString; //imprimir json completo

            ObservableCollection<Medicamento> listaMedicamento = new ObservableCollection<Medicamento>(listaMedicamentos.AsEnumerable<Medicamento>());

            lstMedicamentos.ItemsSource = listaMedicamento;

        }

        private IList<Medicamento> trabajarJson(string contentStr)
        {
            JObject objeto = JObject.Parse(contentStr); //transforma el json en un objeto

            JValue estado = (JValue)objeto["status"]; //obtener el valor del estado
            string strEstado = estado.ToObject<string>(); //pasar a string el estado

            if (!String.Equals(strEstado, "Succeeded"))
            {
                DisplayAlert("¡ERROR!", "EL ANÁLISIS FUE INCORRECTO", "OK");
            }

            JObject recognitionResult = (JObject)objeto["recognitionResult"]; //obtener el objeto recognitionResult que contiene las lineas

            JArray line = (JArray)recognitionResult["lines"]; //pasar las lineas a un jarray

            IList<Line> lineas = line.ToObject<IList<Line>>(); //pasar el jarray a una lista de LINEAS

            int contador = 0;

            IList<Medicamento> listaMedicamentos = new List<Medicamento>(); //lista de medicamentos
          
            Medicamento med = new Medicamento();
            for (int i = 0; i < lineas.Count(); i++) //recorrer la lista de Lineas
            {
                
                if (String.Equals(lineas[i].text, "Medicamento:")) //buscar una frase en una linea
                {
                    contador++;
                    med = new Medicamento();
                    i++;
                    med.posicion = contador;
                    med.nombre = lineas[i].text;
                    listaMedicamentos.Add(med);
                    i--;
                }
                if (String.Equals(lineas[i].text, "Dosificacion:") || String.Equals(lineas[i].text, "Dosification:")) //buscar una frase en una linea
                {
                    i++;
                    med.dosificacion = lineas[i].text;
                    i--;
                }
                if (String.Equals(lineas[i].text, "Cantidad:")) //buscar una frase en una linea
                {
                    i++;
                    med.cantidad = lineas[i].text;
                    i--;
                }
                
                if (String.Equals(lineas[i].text, "Recomendacion: NOCHE")) //buscar una frase en una linea
                {
                    
                    IList<Word> words = lineas[i].words; //pasar el jarray a una lista de WORDS

                    if (String.Equals(words[0].text,"Recomendacion:"))
                    {
                        med.recomendacion = words[1].text;                           
                    }
                      
                    
                }
                if (String.Equals(lineas[i].text, "Recomendacion:") || String.Equals(lineas[i].text, "Recommendation:")) //si detecta unicamente recomendacion
                {
                    IList<Word> words = lineas[i].words;
                    i++;
                    if (words.Count() > 1) //tiene mas de una palabra
                    {
                        if (String.Equals(words[0].text, "Recomendacion:"))
                        {
                            med.recomendacion = words[1].text;
                        }
                        i--;
                    }

                    if (String.Equals(lineas[i].text, "NOCHE")) //si la siguiente es NOCHE
                    {
                        med.recomendacion = "NOCHE";
                        i--;
                    }
                    else
                    {
                        med.recomendacion = "";
                        i--;
                    }
                }

            }

            if(listaMedicamentos.Count()>0){
                DisplayAlert("¡Análisis finalizado!", "Hemos detectado " + listaMedicamentos.Count() + " medicamentos en tu formula médica.", "OK");
                mensaje.Text = "Tu lista de medicamentos es: ";
            }
            else
            {
                DisplayAlert("¡Análisis finalizado!", "No hemos detectado ningun medicamento, toma otra foto por favor.", "OK");
                mensaje.Text = "¡Vuelve a tomar tu foto por favor. Evita que salga borrosa u oscura!";
            }

            return listaMedicamentos;
        }


        private async void lstMedicamentos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var index = e.SelectedItemIndex;
            bool salirWhile = false;
            do
            {
                string action = await DisplayActionSheet("¿Qué deseas hacer?", "Cancelar", null, "Leer el medicamento", "Generar recordatorio");


                if (String.Equals(action, "Leer el medicamento"))
                {
                    leerMedicamento(listaMedicamentos[index]);
                    salirWhile = true;
                }
                if (String.Equals(action, "Generar recordatorio"))
                {
                    generarRecordatorio(listaMedicamentos[index]);
                    salirWhile = true;
                }
                if (String.Equals(action, "Cancelar"))
                {
                    await DisplayAlert("¡ALERTA!", "POR FAVOR ESCOJA UN MEDICAMENTO PARA APLICAR ALGUNA OPCIÓN", "OK");
                    salirWhile = true;
                }
                if (action == null)
                {
                    await DisplayAlert("¡ALERTA!", "POR FAVOR ESCOJA UNA OPCIÓN", "OK");
                }
            }
            while (salirWhile != true);
        }
        private async void leerMedicamento(Medicamento medicamento)
        {
            //implementar el spechh
            await DisplayAlert("¡specc!", "el medicamento es" + medicamento.nombre, "OK");

            
        }
        private async void generarRecordatorio(Medicamento medicamento)
        {
            //implementar el spechh
            await DisplayAlert("¡recorda!", "el medicamento es" + medicamento.nombre, "OK");
        }



    }
    //------------------------clases------------------
    public class Word
    {
        public IList<int> boundingBox { get; set; }
        public string text { get; set; }
        public string confidence { get; set; }
    }

    public class Line
    {
        public IList<int> boundingBox { get; set; }
        public string text { get; set; }
        public IList<Word> words { get; set; }
    }

    public class Medicamento //clase medicamento
    {
        public int posicion { get; set; }
        public string nombre { get; set; }
        public string dosificacion { get; set; }
        public string cantidad { get; set; }
        public string recomendacion { get; set; }
    }

    //----------- fin clases------------------

}
