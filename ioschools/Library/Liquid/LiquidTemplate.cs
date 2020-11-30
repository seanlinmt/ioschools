using System.Globalization;
using System.IO;
using DotLiquid;
using ioschools.Library.Liquid.filters;

namespace ioschools.Library.Liquid
{
    public class LiquidTemplate
    {
        private Template template { get; set; }
        protected Hash parameters { get; set; }

        public LiquidTemplate(string template)
        {
            parameters = new Hash();

            this.template = Template.Parse(template);
        }

        public void AddParameters(string name, object obj)
        {
            parameters.Add(name, obj);
        }

        public string Render()
        {
            var p = new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = parameters,
                Filters = new[]
                                          {
                                              typeof (TextFilter), 
                                              typeof(InputFilter), 
                                              typeof(UrlFilter),
                                              typeof(MoneyFilter)
                                          },
            };

            return template.Render(p);
        }

        public Stream RenderToStream()
        {
            var p = new RenderParameters(CultureInfo.InvariantCulture)
            {
                LocalVariables = parameters,
                Filters = new[]
                                          {
                                              typeof (TextFilter), 
                                              typeof(InputFilter), 
                                              typeof(UrlFilter),
                                              typeof(MoneyFilter)
                                          },
            };
            var ms = new MemoryStream();
            template.Render(ms, p);
            ms.Position = 0;
            return ms;
        }
    }
}