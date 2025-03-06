


using DinoHub.MVVM.Models;

namespace DinoHub.Selectors
{
    public class DinoSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Dinosaurio? dinosaurio = item as Dinosaurio;
            if (dinosaurio != null)
            {
                if (dinosaurio.Carnivoro)
                {
                    Application.Current!.Resources.TryGetValue("DinoCarnivoro", out object CarnivoroTemplate);
                    return CarnivoroTemplate as DataTemplate ?? new DataTemplate();
                }
                else
                {
                    Application.Current!.Resources.TryGetValue("DinoHerviboro", out object HerbivoroTemplate);
                    return HerbivoroTemplate as DataTemplate ?? new DataTemplate(); ;
                }
            }
            return new DataTemplate();
        }
    }
}
