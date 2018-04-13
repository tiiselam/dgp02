using winCompuertaGP.DAL;
using Comun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace winCompuertaGP.BLL
{
    public class Main
    {
        public string connectionString { get; set; }

        // Rango de fechas para generar prefacturas
        public DateTime fechaDesdePref { get; set; }
        public DateTime fechaHastaPref { get; set; }

        public Main(string connectionString)
        {
            this.connectionString = connectionString;
        }

        #region Eventos
        public event EventHandler<ErrorEventArgs> eventoErrorDB;

        protected virtual void OnErrorDB(ErrorEventArgs e)
        {
            //si no es null notificar
            eventoErrorDB?.Invoke(this, e);
        }

        #endregion

        public void setConnectionString(string connection)
        {
            this.connectionString = connection;
        }

        //// Permite comprobar la conexión con la base de datos
        public bool probarConexion()
        {
            using (var db = this.getDbContext())
            {
                return db.Database.Exists();
            }
        }

        public INTEGRAGPEntities getDbContext()
        {
            if (string.IsNullOrEmpty(this.connectionString))
                return new INTEGRAGPEntities();

            return new INTEGRAGPEntities(this.connectionString);
        }

        //// Devuelve las prefacturas que cumplan con los criterios de filtrado seleccionados
        public IList<vwIntegracionesVentas> getPrefacturas( bool filtrarNumPF, string numPFDesde, string numPFHasta, 
                                                            bool filtrarFecha, DateTime fechaDesde, DateTime fechaHasta, 
                                                            bool filtrarEstado, string estado, 
                                                            bool filtrarCliente, string idCliente, 
                                                            bool filtrarReferencia, string referencia, 
                                                            bool filtrarSopnumber, string sopnumberDesde, string sopnumberHasta)
        {
            using (var db = this.getDbContext())
            {
                // verificar la conexión con el servidor de bd
                if (!this.probarConexion())
                {
                    ErrorEventArgs args = new ErrorEventArgs();
                    args.mensajeError = "No se pudo establecer la conexión con el servidor al tratar de leer las pre-facturas.";
                    OnErrorDB(args);
                }

                var datos = db.vwIntegracionesVentas.AsQueryable();

                // Filtrado por número de prefactura
                if (filtrarNumPF)
                {
                    if (numPFDesde != "" )
                    {
                        //int npfDesde = Convert.ToInt32(numPFDesde);
                        datos = datos.Where(m => m.NUMDOCARN.CompareTo(numPFDesde)>= 0 );
                    }

                    if (numPFHasta != "")
                    {
                        //int npfHasta = Convert.ToInt32(numPFHasta);
                        datos = datos.Where(m => m.NUMDOCARN.CompareTo(numPFHasta) <= 0);
                    }
                }

                // Filtrado por fecha
                if (filtrarFecha)
                {
                    datos = datos.Where(m => DbFunctions.TruncateTime(m.FECHADOC) >= fechaDesde && DbFunctions.TruncateTime(m.FECHADOC) <= fechaHasta);
                }

                // Filtrado por estado
                if (filtrarEstado)
                {
                    datos = datos.Where(m => m.DOCSTATUS.Equals(estado));
                }

                //if (filtrarId) {
                //    datos = datos.Where(m => m.ID_PACIENTE.Equals(id));
                //}

                // Filtrado por id de cliente
                if (filtrarCliente && idCliente != "")
                {
                    datos = datos.Where(m => m.IDCLIENTE.Contains(idCliente));
                }

                // Filtrado por referencia
                if (filtrarReferencia && referencia != "")
                {
                    datos = datos.Where(m => m.OBSERVACIONES.Contains(referencia));
                }

                // Filtrado por sopnumbe
                if (filtrarSopnumber && sopnumberDesde != "")
                {
                    //var c = Convert.ToInt32(sopnumberDesde);
                    datos = datos.Where(m => m.NUMDOCGP.CompareTo(sopnumberDesde) >= 0);
                }

                // Filtrado por sopnumbe
                if (filtrarSopnumber && sopnumberHasta != "")
                {
                    //var c = Convert.ToInt32(sopnumberHasta);
                    datos = datos.Where(m => m.NUMDOCGP.CompareTo(sopnumberHasta) <= 0);
                }

                return datos.ToList();
            }
        }

        // Crea las prefacturas en el rango de fechas especificado
        //public async Task<int> crearPrefactura(string empresa)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        return db.itpPreFacturar(fechaDesdePref, fechaHastaPref, empresa);
        //    }
        //}

        //// Devuelve la prefactura que cumpla con el criterio especificado
        //public ITP_PREFACTURA findPrefactura(int id)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        return this.getDbContext().ITP_PREFACTURA.Find(id);
        //    }
        //}

        //// Devuelve el detalle de prefactura que cumpla con el criterio especificado
        //public ITP_DETALLE_PREFACTURA findDetallePrefactura(object[] id)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        return db.ITP_DETALLE_PREFACTURA.Find(id);
        //    }
        //}

        //// Devuelve la prestacion
        //public ITP_PRESTACION findPrestacion(int rowid)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        return db.ITP_PRESTACION.Where(m => m.ROWID == rowid).First();
        //    }
        //}

        //// Devuelve los detalles de las prefacturas que cumplan con los criterios de filtrado seleccionados
        //public IList<ITP_DETALLE_PREFACTURA> getDetallesPrefactura(int numeroPF, bool filtrarDescripcion, string descripcion, bool filtrarCantidad, string cantidadDesde, string cantidadHasta, bool filtrarPrecio, string precioDesde, string precioHasta, bool filtrarUdm, string udm, bool filtrarImporte, string importeDesde, string importeHasta, bool filtrarIdItem, string idItem)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var datos = db.ITP_DETALLE_PREFACTURA.AsQueryable();

        //        datos = datos.Where(m => m.NUMERO_PF == numeroPF);

        //        // Filtrado por descripción
        //        if (filtrarDescripcion && descripcion != "")
        //            datos = datos.Where(m => m.DESCRIPCION.Contains(descripcion));

        //        // Filtrado por cantidad
        //        if (filtrarCantidad && cantidadDesde != "")
        //        {
        //            var c = Convert.ToDecimal(cantidadDesde);
        //            datos = datos.Where(m => m.CANTIDAD >= c);
        //        }

        //        if (filtrarCantidad && cantidadHasta != "")
        //        {
        //            var c = Convert.ToDecimal(cantidadHasta);
        //            datos = datos.Where(m => m.CANTIDAD <= c);
        //        }

        //        // Filtrado por precio
        //        if (filtrarPrecio && precioDesde != "")
        //        {
        //            var c = Convert.ToDecimal(precioDesde);
        //            datos = datos.Where(m => m.PRECIO >= c);
        //        }

        //        if (filtrarPrecio && precioHasta != "")
        //        {
        //            var c = Convert.ToDecimal(precioHasta);
        //            datos = datos.Where(m => m.PRECIO <= c);
        //        }

        //        // Filtrado por Unidad de medida
        //        if (filtrarUdm && udm != "")
        //            datos = datos.Where(m => m.UNIDAD_MEDIDA == udm);

        //        // Filtrado por importe
        //        if (filtrarImporte && importeDesde != "")
        //        {
        //            var c = Convert.ToDecimal(importeDesde);
        //            datos = datos.Where(m => m.IMPORTE >= c);
        //        }

        //        if (filtrarImporte && importeHasta != "")
        //        {
        //            var c = Convert.ToDecimal(importeHasta);
        //            datos = datos.Where(m => m.IMPORTE <= c);
        //        }

        //        // Filtrado por id item
        //        if (filtrarIdItem && idItem != "")
        //            datos = datos.Where(m => m.ID_ITEM == idItem);

        //        return datos.ToList();
        //    }
        //}

        //// Devuelve las prestaciones que cumplan con los criterios de filtrado seleccionados
        //public IList<JoinDataResult> getPrestaciones(bool filtrarDetallePrefactura, ITP_DETALLE_PREFACTURA detallePrefactura, bool filtrarFecha, DateTime fechaDesde, DateTime fechaHasta, bool filtrarModulo, string modulo, bool filtrarPrestacion, string prestacion, bool filtrarLiquidacion, string liquidacion, bool filtrarNombreCliente, string nombreCliente, bool filtrarNombrePaciente, string nombrePaciente, bool filtrarNombrePrestador, string nombrePrestador)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var datos = db.ITP_PRESTACION.AsQueryable();

        //        if (filtrarDetallePrefactura)
        //            datos = datos.Where(m => m.ITP_DETALLE_PREFACTURA.NUMERO_PF == detallePrefactura.NUMERO_PF && m.LINEA_DPF == detallePrefactura.LINEA_DPF);

        //        // Filtrado por fecha
        //        if (filtrarFecha)
        //            datos = datos.Where(m => DbFunctions.TruncateTime(m.FECHA) >= fechaDesde && DbFunctions.TruncateTime(m.FECHA) <= fechaHasta);

        //        // Filtrado por módulo
        //        if (filtrarModulo && !string.IsNullOrEmpty(modulo))
        //            datos = datos.Where(m => m.TIPO_MODULO.Contains(modulo));

        //        // Filtrado por Prestación
        //        if (filtrarPrestacion && !string.IsNullOrEmpty(prestacion))
        //            datos = datos.Where(m => m.TIPO_PRESTACION.Contains(prestacion));

        //        // Filtrado por Id Liquidación
        //        if (filtrarLiquidacion && !string.IsNullOrEmpty(liquidacion))
        //            datos = datos.Where(m => m.CODIGO_LIQUIDACION.Equals(liquidacion));

        //        // Para obtener los nombres de cliente, paciente y prestador
        //        var query = from prest in datos
        //                    from vw in db.vwAADimensionCodes
        //                    .Where(m => m.aaTrxDimcode == prest.CODIGO_PACIENTE).DefaultIfEmpty()
        //                    from pm in db.PM00200
        //                    .Where(m => m.VENDORID == prest.CODIGO_PRESTADOR).DefaultIfEmpty()
        //                    from rm in db.RM00101
        //                    .Where(m => m.USERDEF1 == prest.CODIGO_CLIENTE).DefaultIfEmpty()
        //                    select new JoinDataResult { ITP_PRESTACION = prest, nombrePaciente = vw.aaTrxDimCodeDescr, nombrePrestador = pm.VENDNAME, nombreCliente = rm.CUSTNAME };

        //        // Filtrado por nombre de Cliente
        //        if (filtrarNombreCliente && !string.IsNullOrEmpty(nombreCliente))
        //            query = query.Where(m => m.nombreCliente.Contains(nombreCliente));

        //        // Filtrado por nombre de Prestador
        //        if (filtrarNombrePrestador && !string.IsNullOrEmpty(nombrePrestador))
        //            query = query.Where(m => m.nombrePrestador.Contains(nombrePrestador));

        //        // Filtrado por nombre de Paciente
        //        if (filtrarNombrePaciente && !string.IsNullOrEmpty(nombrePaciente))
        //            query = query.Where(m => m.nombrePaciente.Contains(nombrePaciente));

        //        return query.ToList();
        //    }
        //}

        //// Elimina la prestación especificada
        //public void removePrestacion(int rowid)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var prestacion = db.ITP_PRESTACION.Where(m => m.ROWID == rowid).First();
        //        db.ITP_PRESTACION.Remove(prestacion);
        //        db.SaveChanges();
        //    }
        //}

        //// Elimina la prefactura especificada
        //public void removePrefactura(int pkey)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var prefactura = db.ITP_PREFACTURA.Find(pkey);
        //        db.ITP_PREFACTURA.Remove(prefactura);
        //        db.SaveChanges();
        //    }
        //}

        //// Elimina el detalle de prefactura especificado
        //public void removeDetallePrefactura(object[] pkey)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var detallePrefactura = db.ITP_DETALLE_PREFACTURA.Find(pkey);
        //        db.ITP_DETALLE_PREFACTURA.Remove(detallePrefactura);
        //        db.SaveChanges();
        //    }
        //}

        //public vwIntegracionesVentas findPrefacturaCliente(int numeroPF)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        return db.vwIntegracionesVentas.Find(numeroPF);
        //    }
        //}

        //// Devuelve las prestaciones que están disponibles para ser añadidas a un detalle de prefactura
        //public IList<JoinDataResult> getPrestacionesDisponibles(bool filtrarFecha, DateTime fechaDesde, DateTime fechaHasta, bool filtrarModulo, string modulo, bool filtrarPrestacion, string prestacion, bool filtrarLiquidacion, string liquidacion, bool filtrarNombreCliente, string nombreCliente, bool filtrarNombrePaciente, string nombrePaciente, bool filtrarNombrePrestador, string nombrePrestador)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var datos = db.ITP_PRESTACION.AsQueryable();

        //        datos = datos.Where(m => m.ITP_DETALLE_PREFACTURA == null && m.FACTURAR == "SI");

        //        // Filtrado por fecha
        //        if (filtrarFecha)
        //            datos = datos.Where(m => DbFunctions.TruncateTime(m.FECHA) >= fechaDesde && DbFunctions.TruncateTime(m.FECHA) <= fechaHasta);

        //        // Filtrado por módulo
        //        if (filtrarModulo && !string.IsNullOrEmpty(modulo))
        //            datos = datos.Where(m => m.TIPO_MODULO.Contains(modulo));

        //        // Filtrado por Prestación
        //        if (filtrarPrestacion && !string.IsNullOrEmpty(prestacion))
        //            datos = datos.Where(m => m.TIPO_PRESTACION.Contains(prestacion));

        //        // Filtrado por Id Liquidación
        //        if (filtrarLiquidacion && !string.IsNullOrEmpty(liquidacion))
        //            datos = datos.Where(m => m.CODIGO_LIQUIDACION.Equals(liquidacion));

        //        // Para obtener los nombres de cliente, paciente y prestador
        //        var query = from prest in datos
        //                    from vw in db.vwAADimensionCodes
        //                    .Where(m => m.aaTrxDimcode == prest.CODIGO_PACIENTE).DefaultIfEmpty()
        //                    from pm in db.PM00200
        //                    .Where(m => m.VENDORID == prest.CODIGO_PRESTADOR).DefaultIfEmpty()
        //                    from rm in db.RM00101
        //                    .Where(m => m.USERDEF1 == prest.CODIGO_CLIENTE).DefaultIfEmpty()
        //                    select new JoinDataResult { ITP_PRESTACION = prest, nombrePaciente = vw.aaTrxDimCodeDescr, nombrePrestador = pm.VENDNAME, nombreCliente = rm.CUSTNAME };

        //        // Filtrado por nombre de Cliente
        //        if (filtrarNombreCliente && !string.IsNullOrEmpty(nombreCliente))
        //            query = query.Where(m => m.nombreCliente.Contains(nombreCliente));

        //        // Filtrado por nombre de Prestador
        //        if (filtrarNombrePrestador && !string.IsNullOrEmpty(nombrePrestador))
        //            query = query.Where(m => m.nombrePrestador.Contains(nombrePrestador));

        //        // Filtrado por nombre de Paciente
        //        if (filtrarNombrePaciente && !string.IsNullOrEmpty(nombrePaciente))
        //            query = query.Where(m => m.nombrePaciente.Contains(nombrePaciente));

        //        return query.ToList();
        //    }
        //}

        //public List<ITP_PRESTACION> findPrestaciones(List<int> rowids)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");

        //        var datos = db.ITP_PRESTACION.AsQueryable();
        //        datos = datos.Where(m => rowids.Contains(m.ROWID));

        //        return datos.ToList();
        //    }
        //}

        //public void crearDetallePrefactura(int idPrefactura, List<int> idsPrestaciones, string idItem, string descripcion, string unidadMedida, decimal cantidad, decimal precio, decimal importe)
        //{
        //    using (var db = this.getDbContext())
        //    {
        //        // verificar la conexión con el servidor de bd
        //        if (!this.probarConexion())
        //            throw new Exception("No se pudo establecer la conexión con el servidor.");


        //        var prestaciones = db.ITP_PRESTACION.AsQueryable().Where(m => idsPrestaciones.Contains(m.ROWID)).ToList();
        //        var prefactura = db.ITP_PREFACTURA.Find(idPrefactura);
        //        var minRoiwid = idsPrestaciones.Min();

        //        var detallePrefactura = new ITP_DETALLE_PREFACTURA
        //        {
        //            ITP_PREFACTURA = prefactura,
        //            ITP_PRESTACION = prestaciones,
        //            CANTIDAD = cantidad,
        //            ID_ITEM = idItem,
        //            DESCRIPCION = descripcion,
        //            PRECIO = precio,
        //            IMPORTE = importe,
        //            UNIDAD_MEDIDA = unidadMedida,
        //            NUMERO_PF = idPrefactura,
        //            LINEA_DPF = "P" + minRoiwid
        //        };

        //        db.ITP_DETALLE_PREFACTURA.Add(detallePrefactura);
        //        db.SaveChanges();
        //    }
        //}

    }
}
