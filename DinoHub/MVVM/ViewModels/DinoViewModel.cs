

using DinoHub.MVVM.Models;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using DinoHub.MVVM.Views;

namespace DinoHub.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class DinoViewModel
    {
        //Lista para guardar los dinosaurios
        public ObservableCollection<Dinosaurio> Dinosaurios { get; set; } = new ObservableCollection<Dinosaurio>();


        private HttpClient _httpClient = new HttpClient();
        private JsonSerializerOptions _jsonSerializer = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        //Dirección base donde van a estar los endpoints
        private String _url = "https://67acdb103f5a4e1477dc1658.mockapi.io/api/v1";

        //Establecemos variables para el entry de la ID
        private string _dinoId;
        public string DinoId
        {
            get => _dinoId;
            set
            {
                _dinoId = value;

            }
        }

        //variables para crear un nuevo dinosaurio
        private bool _mostrarFormulario;
        private Dinosaurio? _nuevoDinosaurio;

        // Propiedad para mostrar u ocultar el formulario
        public bool MostrarFormulario { get; set; }

        // Dinosaurio que el usuario está creando
        public Dinosaurio? NuevoDinosaurio { get; set; }
        public Dinosaurio? DinosaurioEditable { get; set; }
        public DinoViewModel() 
        {
            NuevoDinosaurio = new Dinosaurio(); // Inicialización en el constructor
        }

        public DinoViewModel(Dinosaurio dinosaurio)
        {
            DinosaurioEditable = new Dinosaurio
            {
                Id = dinosaurio.Id,
                Nombre = dinosaurio.Nombre,
                Tamano = dinosaurio.Tamano,
                Imagen = dinosaurio.Imagen,
                Carnivoro = dinosaurio.Carnivoro
            };
        }
        //SACAR TODA LA LISTA DE LOS DINOSAURIOS
        #region ListaDinos
        public ICommand GetAllDinosCommand
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        // Cogemos la url y el get sería lo que nos dice la página de mockAPI
                        string url = $"{_url}/Dinosaurios";
                        HttpResponseMessage response = await _httpClient.GetAsync(url);

                        if (response.IsSuccessStatusCode)  // Si la respuesta ha sido satisfactoria
                        {
                            // Sacamos los datos
                            using (var data = await response.Content.ReadAsStreamAsync())
                            {
                                List<Dinosaurio>? dinosFromJson = await JsonSerializer.DeserializeAsync<List<Dinosaurio>>(data, _jsonSerializer);
                                if (dinosFromJson != null)
                                {
                                    Dinosaurios.Clear();
                                    // Metemos los dinosaurios en la lista
                                    dinosFromJson.ForEach(dino => Dinosaurios.Add(dino));
                                    Console.WriteLine("Dinosaurios cargados correctamente.");
                                }
                                else
                                {
                                    Console.WriteLine("No se pudieron deserializar los datos.");
                                }
                            }
                        }
                        else // Si la respuesta no ha sido satisfactoria
                        {
                            Console.WriteLine($"Error en la respuesta del servidor: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Excepción al obtener los dinosaurios: {ex.Message}");
                    }
                });
            }
        } 
        #endregion
        //SACAR NOMBRE DEL DINOSAURIO POR SU ID
        #region DinosaurioByID
        public ICommand GetDinoByIdCommand => new Command(async (id) =>
       {
           Dinosaurio? dino = null;
           try
           {
               //Hay que pasarle como identificador el id
               var url = $"{_url}/Dinosaurios/{id}";
               //Sacamos un usuario
               dino = await _httpClient.GetFromJsonAsync<Dinosaurio>(url, _jsonSerializer);
               if (dino != null)
               {
                   await Application.Current.MainPage.DisplayAlert("Dinosaurio Encontrado", $"Nombre: {dino.Nombre}", "OK");
               }

           }
           catch (Exception ex)
           {
               await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el dinosaurio", "OK");
           }
       }); 
        #endregion

        public ICommand NavigateAddPageCommand => new Command(async () =>
        {
            // Navegar directamente a la página para añadir un nuevo dinosaurio
            var addPage = new AddPage();
            await Application.Current.MainPage.Navigation.PushAsync(addPage);
        });

        public ICommand NavigateEditPageCommand => new Command(async (id) =>
        {
            // Validar que haya una ID ingresada
            if (string.IsNullOrEmpty(id?.ToString()))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Introduce un ID válido.", "OK");
                return;
            }

            // Intentar convertir la ID a un número
            if (!long.TryParse(id.ToString(), out long dinoId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "ID inválido. Debe ser un número.", "OK");
                return;
            }

            // Buscar el dinosaurio por ID
            var dinosaurio = Dinosaurios.FirstOrDefault(d => d.Id == dinoId);

            if (dinosaurio != null)
            {
                // Si existe, abrir la página de edición con los datos ya cargados
                var editPage = new EditPage(dinosaurio);
                await Application.Current.MainPage.Navigation.PushAsync(editPage);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el dinosaurio con esa ID.", "OK");
            }
        });



        //AÑADIR DINOSAURIO
        #region AddDino
        public ICommand AddDinoCommand => new Command(async () =>
        {
            // Si el formulario está visible, intentamos guardar el dinosaurio
            if (MostrarFormulario)
            {
                /* Validar si los campos son válidos
                if (string.IsNullOrEmpty(NuevoDinosaurio.Nombre) || NuevoDinosaurio.Tamano <=0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
                    return;
                }*/

                // Obtener el próximo ID automáticamente
                var nextId = await ObtenerProximoIdDinosaurio();

                // Asignar el ID automáticamente al dinosaurio
                NuevoDinosaurio.Id = nextId;

                // Crear el objeto JSON
                var url = $"{_url}/Dinosaurios";
                var json = JsonSerializer.Serialize(NuevoDinosaurio, _jsonSerializer);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");


                // Enviar la solicitud al servidor para guardar el dinosaurio
                var response = await _httpClient.PostAsync(url, content);

                // Mostrar mensaje de éxito o error
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Dinosaurio añadido correctamente", "OK");
                    MostrarFormulario = false; // Ocultar el formulario después de guardar
                    NuevoDinosaurio = new Dinosaurio(); // Limpiar el formulario
                    try
                    {
                        await Application.Current.MainPage.Navigation.PopAsync(); // Volvemos a la pantalla principal
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error en la navegación", $"Hubo un error al volver a la página principal: {ex.Message}", "OK");
                    }

                    //recargar la lista de dinosaurios
                    try
                    {
                        // Asegurarse de que MainPage es un NavigationPage
                        var mainPage = Application.Current.MainPage;

                        // Si es un NavigationPage, obtenemos la página actual dentro de él
                        if (mainPage is NavigationPage navigationPage)
                        {
                            // Ahora obtenemos la página principal que está dentro del NavigationPage
                            mainPage = navigationPage.CurrentPage;
                        }

                        // Aseguramos que ahora mainPage sea de tipo MainPage
                        if (mainPage is MainPage actualMainPage)
                        {
                            if (actualMainPage.BindingContext is DinoViewModel mainPageViewModel)
                            {
                                // Ejecutamos el comando para recargar los dinosaurios
                                mainPageViewModel.GetAllDinosCommand.Execute(null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error al recargar", $"Hubo un error al recargar la lista de dinosaurios: {ex.Message}", "OK");
                    }

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Hubo un problema al añadir el dinosaurio", "OK");
                }
            }
            else
            {
                // Si no está visible, mostrar el formulario
                MostrarFormulario = true;
            }
        });

        // Método para obtener el próximo ID del dinosaurio
        private async Task<long> ObtenerProximoIdDinosaurio()
        {

            var url = $"{_url}/Dinosaurios";
            var dinosaurios = await _httpClient.GetFromJsonAsync<List<Dinosaurio>>(url);
            return dinosaurios.Max(dino => dino.Id) + 1;
        } 
        #endregion



        public ICommand UpdateDinoCommand => new Command(async () =>
    {
        // Validar que el dinosaurio a editar existe
        if (NuevoDinosaurio == null || string.IsNullOrEmpty(NuevoDinosaurio.Nombre) || NuevoDinosaurio.Tamano <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor complete todos los campos antes de actualizar.", "OK");
            return;
        }

        var url = $"{_url}/Dinosaurios/{NuevoDinosaurio.Id}"; // URL del dinosaurio a actualizar

        // Serializamos el dinosaurio actualizado a JSON
        var json = JsonSerializer.Serialize(NuevoDinosaurio, _jsonSerializer);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        // Enviar la solicitud PUT para actualizar el dinosaurio
        var response = await _httpClient.PutAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            // Si la respuesta es exitosa, mostramos un mensaje
            await Application.Current.MainPage.DisplayAlert("Éxito", "Dinosaurio actualizado correctamente", "OK");
            await Application.Current.MainPage.Navigation.PopAsync(); // Volver a la pantalla principal
        }
        else
        {
            // Si hubo algún problema, mostramos un mensaje de error
            await Application.Current.MainPage.DisplayAlert("Error", "Hubo un problema al actualizar el dinosaurio", "OK");
        }
    });

        public ICommand SaveCommand => new Command(async () =>
        {
            if (DinosaurioEditable == null || string.IsNullOrEmpty(DinosaurioEditable.Nombre) || DinosaurioEditable.Tamano <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Completa todos los campos antes de actualizar.", "OK");
                return;
            }

            var url = $"{_url}/Dinosaurios/{DinosaurioEditable.Id}";
            var json = JsonSerializer.Serialize(DinosaurioEditable);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Dinosaurio actualizado correctamente", "OK");

                // 🔹 Volver atrás
                await Application.Current.MainPage.Navigation.PopAsync();

                // 🔹 Recargar la lista de dinosaurios
                try
                {
                    var mainPage = Application.Current.MainPage;

                    if (mainPage is NavigationPage navigationPage)
                    {
                        mainPage = navigationPage.CurrentPage;
                    }

                    if (mainPage is MainPage actualMainPage && actualMainPage.BindingContext is DinoViewModel mainViewModel)
                    {
                        mainViewModel.GetAllDinosCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error al recargar", $"Hubo un error al actualizar la lista: {ex.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo actualizar el dinosaurio", "OK");
            }
        });

        public ICommand DeleteDinoCommand => new Command(async () =>
        {
            if (string.IsNullOrEmpty(DinoId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Introduce un ID válido.", "OK");
                return;
            }

            var url = $"{_url}/Dinosaurios/{DinoId}";

            var response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Dinosaurio eliminado correctamente", "OK");

                // 🔹 Recargar la lista después de eliminar
                try
                {
                    var mainPage = Application.Current.MainPage;

                    if (mainPage is NavigationPage navigationPage)
                    {
                        mainPage = navigationPage.CurrentPage;
                    }

                    if (mainPage is MainPage actualMainPage && actualMainPage.BindingContext is DinoViewModel mainViewModel)
                    {
                        mainViewModel.GetAllDinosCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error al recargar", $"Hubo un error al actualizar la lista: {ex.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar el dinosaurio", "OK");
            }
        });





    }

}

