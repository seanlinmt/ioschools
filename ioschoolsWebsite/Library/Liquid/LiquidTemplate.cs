using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using DotLiquid;
using ioschoolsWebsite.Library.Liquid.filters;

namespace ioschoolsWebsite.Library.Liquid
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
            var p = new RenderParameters()
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
            var p = new RenderParameters()
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