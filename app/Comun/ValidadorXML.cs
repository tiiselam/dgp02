using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace Comun
{

public class ValidadorXML
{
    //public int numErrores = 0;             // Validation Error Count
    //public string mensajeError = "";        // Validation Error Message
    private XmlSchemaSet sc;                // Esquema

    public ValidadorXML(Parametros prm)
    {
        // Create the XmlSchemaSet class.
        sc = new XmlSchemaSet();
        try
        {
            // Add the schema to the collection.
            sc.Add(null, prm.URLArchivoXSD);
            sc.Add(null, prm.URLArchivoPagosXSD);

            if (!prm.URLArchivoComExtXSD.ToLower().Equals("na"))
                    sc.Add(null, prm.URLArchivoComExtXSD);
            }
        catch(Exception vx)
        {
            throw new IOException ("No existe alguno de los archivos xsd: \r\n" + prm.URLArchivoXSD + " o \r\n" + prm.URLArchivoPagosXSD + "\r\n", vx);
        }
    }

    // Display any warnings or errors.
    private void ValidationCallBack(object sender, ValidationEventArgs args)
    {
        if (args.Severity == XmlSeverityType.Warning)
            throw new ArgumentException("Adventencia de validación del esquema del archivo xml. " + args.Message);
        else
            throw new ArgumentException("Error de validación del esquema del archivo xml. " + args.Message);
    }

    public void ValidarXSD(XmlDocument archivoXml)
    {
        XmlNodeReader nodeReader = new XmlNodeReader(archivoXml);

        // Set the validation settings.
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.Schema;
        settings.Schemas = sc;
        settings.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);

        try
        {
            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(nodeReader, settings);
            // Parse the file. 
            while (reader.Read()) ;

        }
        catch 
        {
            throw;
        }
    }
}

}
