using System.Web.Mvc;

namespace ioschools.Areas.exams
{
    public class examsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "exams";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
                "Single Exam View",                                              // Route name
                "exams/{id}",                           // URL with parameters
                new { controller = "Exams", action = "Single" },
                new { id = @"\d+" }
            );

            context.MapRoute(
                "Exam Templates",
                "exams/templates/{action}/{id}",
                new { controller = "templates", action = "Index", id = UrlParameter.Optional, Area = "exams" }
            );

            context.MapRoute(
                "exams_default",
                "exams/{action}/{id}",
                new { controller = "exams", action = "Index", id = UrlParameter.Optional, Area = "exams" }
            );


        }
    }
}