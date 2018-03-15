using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;

namespace Comun
{
    public class TransformerXML
    {
        private Dictionary<string, XslCompiledTransform> transforms = new Dictionary<string, XslCompiledTransform>();
        public string cadenaOriginal = string.Empty;
        public string cadenaOriginalComex = string.Empty;
        private XslCompiledTransform _xslCfdiCompilado = null;
        private Parametros _param;

        public TransformerXML(Parametros pa)
        {
            _param = pa;
            Load(_param.URLArchivoXSLT);
        }

        public TransformerXML(string rutaArchivoXSLT)
        {
            Load(rutaArchivoXSLT);
        }

        public void Load(string rutaArchivoXSLT)
        {
            //XslCompiledTransform transform = null;
            try
            {
                if (!transforms.TryGetValue(rutaArchivoXSLT, out _xslCfdiCompilado))
                {
                    _xslCfdiCompilado = new XslCompiledTransform();
                    _xslCfdiCompilado.Load(rutaArchivoXSLT);
                    transforms[rutaArchivoXSLT] = _xslCfdiCompilado;
                }
                //return transform;
            }
            catch (Exception lo)
            {
                throw new IOException("Excepción al inicializar la plantilla de transformación de XML. Verifique la existencia del archivo: " + rutaArchivoXSLT, lo);
            }
        }

        /// <summary>
        /// Transforma el xml a cadena original
        /// </summary>
        /// <param name="archivoXml">Archivo xml a transformar.</param>
        /// <param name="transformer">Objeto que aplica un xslt al archivo xml.</param>
        /// <returns>False cuando hay al menos un error</returns>
        public string getCadenaOriginal(XmlDocument archivoXml, XslCompiledTransform transformer)
        {
            StringWriter writer = new StringWriter();
            try
            {
                transformer.Transform(archivoXml, null, writer);
                return(writer.ToString());
            }
            catch 
            {
                throw;
            }
        }

        public void getCadenaOriginal(XmlDocument comprobanteCfdiXml)
        {
            cadenaOriginal = getCadenaOriginal(comprobanteCfdiXml, _xslCfdiCompilado);

        }
    }
}
