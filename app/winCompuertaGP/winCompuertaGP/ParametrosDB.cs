using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegradorDeGP;
using System.Xml;

namespace winCompuertaGP
{
    public struct Empresa
    {
        private string idbd;
        private string nombreBd;
        private string metadataIntegra;
        private string metadataGP;
        private string metadataUIIntegra;

        public string Idbd
        {
            get
            {
                return idbd;
            }

            set
            {
                idbd = value;
            }
        }

        public string NombreBd
        {
            get
            {
                return nombreBd;
            }

            set
            {
                nombreBd = value;
            }
        }

        /// <summary>
        /// metadata de la bd Integra del servicio de integración
        /// </summary>
        public string MetadataIntegra
        {
            get
            {
                return metadataIntegra;
            }

            set
            {
                metadataIntegra = value;
            }
        }

        /// <summary>
        /// metadata de la bd GP del servicio de integración
        /// </summary>
        public string MetadataGP
        {
            get
            {
                return metadataGP;
            }

            set
            {
                metadataGP = value;
            }
        }

        /// <summary>
        /// metadata de la bd Integra de la aplicación winForms
        /// </summary>
        public string MetadataUIIntegra
        {
            get
            {
                return metadataUIIntegra;
            }

            set
            {
                metadataUIIntegra = value;
            }
        }
    }

    public class ParametrosDB:IParametrosDB
    {
        private List<Empresa> _empresas;
        private string nombreArchivoParametros = "ParametrosCompuertaGP.xml";
        private string targetGPDB = "";
        private string _servidor = "";
        private string _seguridadIntegrada = "0";
        private string _usuarioSql = "";
        private string _passwordSql = "";
        private string connStringSourceEFUI = string.Empty;
        private string connectionStringSourceEF = string.Empty;
        private string connectionStringTargetEF = string.Empty;
        private string connStringSource = string.Empty;
        private string connStringTarget = string.Empty;
        private string formatoFechaDB;
        private string rutaLog;
        Dictionary<string, string> idsDocumento;

        public ParametrosDB()
        {
            //try
            //{
                XmlDocument listaParametros = new XmlDocument();
                listaParametros.Load(new XmlTextReader(nombreArchivoParametros));

                this._servidor = listaParametros.DocumentElement.SelectSingleNode("/listaParametros/servidor/text()").Value;
                this.DefaultDB = listaParametros.DocumentElement.SelectSingleNode("/listaParametros/servidor").Attributes["defaultDB"].Value;
                this._seguridadIntegrada = listaParametros.DocumentElement.SelectSingleNode("/listaParametros/seguridadIntegrada/text()").Value;
                this._usuarioSql = listaParametros.DocumentElement.SelectSingleNode("/listaParametros/usuariosql/text()").Value;
                this._passwordSql = listaParametros.DocumentElement.SelectSingleNode("/listaParametros/passwordsql/text()").Value;

                XmlNodeList empresasNodes = listaParametros.DocumentElement.SelectNodes("/listaParametros/compannia");

                this._empresas = new List<Empresa>();
                foreach (XmlNode empresaNode in empresasNodes)
                {
                    this._empresas.Add(new Empresa()
                    {
                        Idbd = empresaNode.Attributes["bd"].Value,
                        NombreBd = empresaNode.Attributes["nombre"].Value,
                        MetadataIntegra = empresaNode.Attributes["metadataIntegra"].Value,
                        MetadataGP = empresaNode.Attributes["metadataGP"].Value,
                        MetadataUIIntegra = empresaNode.Attributes["metadataUI"].Value
                    });
                }

            //}
            //catch (Exception eprm)
            //{
            //    ultimoMensaje = "Contacte al administrador. No se pudo obtener la configuración general. [Parametros()]" + eprm.Message;
            //}
        }

        public void GetParametros(int idxEmpresa)
        {
            string IdCompannia = this._empresas[idxEmpresa].Idbd;
                XmlDocument listaParametros = new XmlDocument();
                listaParametros.Load(new XmlTextReader(nombreArchivoParametros));
                XmlNode elemento = listaParametros.DocumentElement;


            FormatoFechaDB = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/formatoFechaDB/text()").Value;
            targetGPDB = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/TargetGPDB/text()").Value;
            if (seguridadIntegrada)
            {
                connectionStringSourceEF = this._empresas[idxEmpresa].MetadataIntegra + "provider connection string='data source=" + _servidor + "; initial catalog = " + IdCompannia + "; integrated security = True; MultipleActiveResultSets = True; App = EntityFramework'";
                connectionStringTargetEF = this._empresas[idxEmpresa].MetadataGP + "provider connection string='data source=" + _servidor + "; initial catalog = " + targetGPDB + "; integrated security = True; MultipleActiveResultSets = True; App = EntityFramework'";
                connStringSource = "Initial Catalog=" + IdCompannia + ";Data Source=" + _servidor + ";Integrated Security=SSPI";
                connStringTarget = "Initial Catalog=" + targetGPDB + ";Data Source=" + _servidor + ";Integrated Security=SSPI";
                connStringSourceEFUI = this._empresas[idxEmpresa].MetadataUIIntegra + "provider connection string='data source=" + _servidor + "; initial catalog = " + IdCompannia + "; integrated security = True; MultipleActiveResultSets = True; App = EntityFramework'"; 
            }
            else
            {
                connectionStringSourceEF = this._empresas[idxEmpresa].MetadataIntegra + "provider connection string='data source=" + _servidor + ";initial catalog=" + IdCompannia + ";user id=" + _usuarioSql + ";Password=" + _passwordSql + ";integrated security=False; MultipleActiveResultSets=True;App=EntityFramework'";
                connectionStringTargetEF = this._empresas[idxEmpresa].MetadataGP + "provider connection string='data source=" + _servidor + ";initial catalog=" + targetGPDB + ";user id=" + _usuarioSql + ";Password=" + _passwordSql + ";integrated security=False; MultipleActiveResultSets=True;App=EntityFramework'";
                connStringSource = "User ID=" + _usuarioSql + ";Password=" + _passwordSql + ";Initial Catalog=" + IdCompannia + ";Data Source=" + _servidor;
                connStringTarget = "User ID=" + _usuarioSql + ";Password=" + _passwordSql + ";Initial Catalog=" + targetGPDB + ";Data Source=" + _servidor;
                connStringSourceEFUI = this._empresas[idxEmpresa].MetadataUIIntegra + "provider connection string='data source=" + _servidor + "; initial catalog = " + IdCompannia + ";user id=" + _usuarioSql + ";Password=" + _passwordSql + ";integrated security=False; MultipleActiveResultSets=True;App=EntityFramework'";
            }

            RutaLog = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/RutaLog/text()").Value;
            XmlNodeList idsDocumentoSOP = listaParametros.DocumentElement.SelectNodes("/listaParametros/compannia[@bd='" + IdCompannia + "']/idsDocumentoSOP");
            IdsDocumento = new Dictionary<string, string>();
            foreach (XmlNode n in idsDocumentoSOP)
            {
                try
                {
                    IdsDocumento.Add(n.Attributes["idAriane"].Value, n.Attributes["idGP"].Value);
                }
                catch
                { }
            }
        }

        public string servidor
        {
            get { return _servidor; }
            set { _servidor = value; }
        }

        public bool seguridadIntegrada
        {
            get
            {
                return _seguridadIntegrada.Equals("1");
            }
            set
            {
                if (value)
                    _seguridadIntegrada = "1";
                else
                    _seguridadIntegrada = "0";
            }
        }

        public string usuarioSql
        {
            get { return _usuarioSql; }
            set { _usuarioSql = value; }
        }

        public string passwordSql
        {
            get { return _passwordSql; }
            set { _passwordSql = value; }
        }


        public string TargetGPDB
        {
            get
            {
                return targetGPDB;
            }
            set { targetGPDB = value; }

        }


        public List<Empresa> Empresas
        {
            get
            {
                return _empresas;
            }

            set
            {
                _empresas = value;
            }
        }

        public string ConnectionStringSourceEF
        {
            get
            {
                return connectionStringSourceEF;
            }

            set
            {
                connectionStringSourceEF = value;
            }
        }

        public string ConnectionStringTargetEF
        {
            get
            {
                return connectionStringTargetEF;
            }

            set
            {
                connectionStringTargetEF = value;
            }
        }
        public string DefaultDB { get; private set; }

        public string ConnStringSource
        {
            get
            {
                return connStringSource;
            }

            set
            {
                connStringSource = value;
            }
        }

        public string ConnStringTarget
        {
            get
            {
                return connStringTarget;
            }

            set
            {
                connStringTarget = value;
            }
        }

        public string FormatoFechaDB
        {
            get
            {
                return formatoFechaDB;
            }

            set
            {
                formatoFechaDB = value;
            }
        }

        public string RutaLog
        {
            get
            {
                return rutaLog;
            }

            set
            {
                rutaLog = value;
            }
        }

        public string ConnStringSourceEFUI
        {
            get
            {
                return connStringSourceEFUI;
            }

            set
            {
                connStringSourceEFUI = value;
            }
        }

        public Dictionary<string, string> IdsDocumento
        {
            get
            {
                return idsDocumento;
            }

            set
            {
                idsDocumento = value;
            }
        }
    }

}

