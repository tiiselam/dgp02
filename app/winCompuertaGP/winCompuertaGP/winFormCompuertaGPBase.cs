using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
//using preFacturaTeamPediatrico.DAL;
using winCompuertaGP.BLL;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using Comun;
using IntegradorDeGP;

namespace winCompuertaGP
{
    public partial class winFormCompuertaGPBase : Form
    {
        private Main mainController;
        private int idPrefacturaSeleccionada;
        private ParametrosDB configuracion;
        private object[] idDetallePrefacturaSeleccionada;

        private object celdaActual;
        private List<int> filasActualizadas;

        DateTime fechaIni = DateTime.Today;
        DateTime fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);

        int dePeriodo = DateTime.Now.Year * 100 + 01;
        int aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;

        bool filtrarDetallePrefactura;

        public winFormCompuertaGPBase()
        {
            try
            {
                InitializeComponent();

                //dgvFacturas.AutoGenerateColumns = false;

                cmbBEstado.SelectedIndex = 0;


                mainController = new Main("");
                mainController.eventoErrorDB += MainController_eventoErrorDB;
                configuracion = new ParametrosDB();
                cargarEmpresas();
                
                var empresaDefault = configuracion.Empresas.Where(x => x.Idbd == configuracion.DefaultDB).First();
                cmbBxCompannia.SelectedIndex = configuracion.Empresas.IndexOf(empresaDefault);

                celdaActual = null;
                filasActualizadas = new List<int>();

                lblFecha.Text = DateTime.Now.ToShortDateString();
                lblUsuario.Text = Environment.UserName;

                this.idPrefacturaSeleccionada = -1;
                this.idDetallePrefacturaSeleccionada = new object[2];
                this.filtrarDetallePrefactura = false;
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = string.Concat( exc.Message, Environment.NewLine, exc?.InnerException?.ToString(), Environment.NewLine, exc.StackTrace , Environment.NewLine);
            }

        }

        private void MainController_eventoErrorDB(object sender, ErrorEventArgs e)
        {
            txtbxMensajes.Text += e.mensajeError + Environment.NewLine;
        }

        private void cargarEmpresas()
        {
            try
            {
                cmbBxCompannia.Items.Clear();
                foreach (Empresa e in configuracion.Empresas)
                {
                    cmbBxCompannia.Items.Add(e.Idbd + "->" + e.NombreBd);
                }
            }
            catch (Exception exc)
            {
                txtbxMensajes.AppendText(exc.Message + Environment.NewLine);
            }
        }

        private void cmbBxCompannia_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarDatosEmpresa(((ComboBox)sender).SelectedIndex);
        }

        private void winformPreFactura_Load(object sender, EventArgs e)
        {

        }

        private void cargarDatosEmpresa(int index)
        {

            // Limpiar los DataGridViews
            dgvFacturas.Rows.Clear();
            //dgvDetallesPrefactura.Rows.Clear();
            //dgvPrestaciones.Rows.Clear();

            // Limpiar los filtros y las cabeceras
            limpiarCabecerasDetallesPrefacturas();
            limpiarCabecerasPrestaciones();
            limpiarFiltrosPreFacturas();
            limpiarFiltrosDetallesPrefacturas();
            limpiarFiltrosPrestaciones();

            // Establecer el nuevo string de conexión
            //mainController.connectionString = this.connections[index];
            configuracion.GetParametros(index);
            mainController.connectionString = configuracion.ConnStringSourceEFUI;

            // Limpiar los mensajes
            txtbxMensajes.Text = "";

            // Verificar la conexión
            if (mainController.probarConexion())
                // Recargar los datos del grid
                filtrarPreFacturas();
            else
                txtbxMensajes.Text = "Contacte al administrador. No se pudo establecer la conexión para la compañía seleccionada. [cargarDatosEmpresa]";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                txtbxMensajes.Text = "";
                var errores = validarFiltrosPreFacturas();
                if (errores == "")
                {
                    var c = filtrarPreFacturas();
                    txtbxMensajes.Text = "";
                    txtbxMensajes.AppendText("Total de documentos encontrados: " + c + Environment.NewLine);
                }
                else
                    txtbxMensajes.Text = errores;
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = string.Concat( exc.Message , Environment.NewLine , exc?.InnerException.ToString(), Environment.NewLine);
            }
        }

        private void reportaProgreso(int i, string s)
        {
            //iProgreso = i;
            tsProgressBar1.Increment(i);
            //tsProgressBar1.Refresh();

            if (tsProgressBar1.Value == tsProgressBar1.Maximum)
                tsProgressBar1.Value = 0;

            txtbxMensajes.AppendText(s + "\r\n");
            txtbxMensajes.Refresh();
        }

        private void reportaAlertas(string s)
        {
            textBoxAlertas.AppendText(s + "\r\n");
            textBoxAlertas.Refresh();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            prefacturar();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void hoytsMenuItem4_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today;  
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = hoytsMenuItem4.Text;
            filtrarPreFacturas();
        }

        private void ayertsMenuItem5_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-1);
            fechaFin = DateTime.Today.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ayertsMenuItem5.Text;
            filtrarPreFacturas();
        }

        private void ultimos7tsMenuItem6_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-6);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos7tsMenuItem6.Text;
            filtrarPreFacturas();
        }

        private void ultimos30tsMenuItem7_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-29);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos30tsMenuItem7.Text;
            filtrarPreFacturas();
        }

        private void ultimos60tsMenuItem8_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-59);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos60tsMenuItem8.Text;
            filtrarPreFacturas();
        }

        private void mesActualtsMenuItem9_Click(object sender, EventArgs e)
        {
            fechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fechaFin = fechaIni.AddMonths(1);
            int ultimoDia = fechaFin.Day;
            fechaFin = fechaFin.AddDays(-ultimoDia);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = mesActualtsMenuItem9.Text;
            filtrarPreFacturas();
        }

        private void tsDropDownFiltro_TextChanged(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
        }



        private void btnAlicarFiltroLibros_Click(object sender, EventArgs e)
        {
            try
            {
                txtbxMensajes.Text = "";
                var errores = validarFiltrosPrefactura();
                if (errores == "")
                    filtrarDetallesPrefactura();
                else
                    txtbxMensajes.Text = errores;
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = exc.Message + "\r\n";
            }
        }


        private void tsRechazar_Click(object sender, EventArgs e)
        {
            //toolStripConfirma2.Visible = true;
        }

        private int filtrarPreFacturas()
        {
            bool cbFechaMarcada = checkBoxFecha.Checked;
            DateTime fini = dtPickerDesde.Value.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
            DateTime ffin = dtPickerHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!checkBoxPacientes_numero_pf.Checked && !checkBoxFecha.Checked && !checkBoxEstado.Checked && !checkBoxPacientes_nombre_cliente.Checked && !checkBoxPacientes_referencia.Checked && !checkBoxPacientes_sopnumbe.Checked)
            {
                cbFechaMarcada = true;
                fini = fechaIni;
                ffin = fechaFin;
            }

            var datos = mainController.getPrefacturas(
                                                        checkBoxPacientes_numero_pf.Checked,
                                                        textBoxPacientes_numero_pf_desde.Text,
                                                        textBoxPacientes_numero_pf_hasta.Text,
                                                        cbFechaMarcada,
                                                        fini,
                                                        ffin,
                                                        checkBoxEstado.Checked,
                                                        cmbBEstado.SelectedItem.ToString(),
                                                        checkBoxPacientes_nombre_cliente.Checked,
                                                        textBoxPacientes_nombre_cliente.Text,
                                                        checkBoxPacientes_referencia.Checked,
                                                        textBoxPacientes_referencia.Text,
                                                        checkBoxPacientes_sopnumbe.Checked,
                                                        textBoxPacientes_sopnumbe_desde.Text,
                                                        textBoxPacientes_sopnumbe_hasta.Text
                                                    );
            bindingSource1.DataSource = datos;
            dgvFacturas.AutoGenerateColumns = false;
            dgvFacturas.DataSource = bindingSource1;
            dgvFacturas.AutoResizeColumns();
            //dgvFacturas.RowHeadersVisible = false;

            return datos.Count;
        }

        private void dgvPacientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //var row = e.RowIndex;
            //// validar que el doble click no sea en la cabecera
            //if (row != -1)
            //{
            //    try
            //    {
            //        int numeroPF = Convert.ToInt32(((DataGridView)sender).Rows[row].Cells[1].Value);
            //        limpiarFiltrosDetallesPrefacturas();
            //        var prefactura = mainController.findPrefactura(numeroPF);
            //        if (prefactura != null)
            //        {
            //            this.idPrefacturaSeleccionada = prefactura.NUMERO_PF;
            //            mostrarDetallesPrefactura(prefactura);
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //}
        }

        private void tsBtnIntegraFactura_Click(object sender, EventArgs e)
        {
            try
            {
                //tsProgressBar1.Style = ProgressBarStyle.Marquee;
                //tsProgressBar1.MarqueeAnimationSpeed = 40;

                IntegraVentasBandejaDB bandejaDB = new IntegraVentasBandejaDB(configuracion, Environment.UserName);

                bandejaDB.eventoProgreso += reportaProgreso;   // BandejaDB_eventoProgreso;

                bandejaDB.ProcesaBandejaDB("LISTO", "ENVIAR_A_GP");

                filtrarPreFacturas();


            }
            catch (Exception ex)
            {
                reportaProgreso(0, ex.Message);
            }
            finally
            {
                //tsProgressBar1.Style = ProgressBarStyle.Continuous;
                //tsProgressBar1.MarqueeAnimationSpeed = 0;
            }

        }

        //private void mostrarDetallesPrefactura(ITP_PREFACTURA prefactura)
        //{
        //    if (prefactura != null)
        //    {
        //        dgvDetallesPrefactura.Rows.Clear();
        //        foreach (var item in prefactura.ITP_DETALLE_PREFACTURA)
        //        {
        //            object[] row = {
        //                false,
        //                item.NUMERO_PF, // columnas ocultas para obtener pkey
        //                item.LINEA_DPF, // columnas ocultas para obtener pkey
        //                item.ID_ITEM,
        //                item.DESCRIPCION,
        //                item.UNIDAD_MEDIDA,
        //                item.CANTIDAD.ToString(),
        //                item.PRECIO.ToString(),
        //                item.IMPORTE.ToString()
        //            };
        //            dgvDetallesPrefactura.Rows.Add(row);
        //        }
        //        tabControlPreFactura.SelectTab(1);
        //        llenarCabecerasDetallesPrefacturas(prefactura);
        //        dgvDetallesPrefactura.Refresh();
        //    }
        //}

        private void dgvDetallesPrefactura_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // validar que el cambio no sea el checkbox
            if (e.ColumnIndex != 0)
                celdaActual = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dgvDetallesPrefactura_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validar que el cambio no sea el checkbox
            if (e.ColumnIndex != 0)
            {
                object objetoFinal = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                bool dif = false;
                if (celdaActual != null)
                    dif = !celdaActual.Equals(objetoFinal);
                else
                    dif = celdaActual != objetoFinal;

                if (dif)
                {
                    //MessageBox.Show("Hay cambios");
                    if (!filasActualizadas.Contains(e.RowIndex))
                    {
                        filasActualizadas.Add(e.RowIndex);
                        comprobarCambiosDetallesPrefactura();
                    }

                    // Cambiar el color de la celda en caso de que existan cambios
                    var estilo = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Style;
                    estilo.ForeColor = Color.Blue;
                    ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Style = estilo;
                }
            }
        }

        private void dgvPrestaciones_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // validar que el cambio no sea el checkbox
            if (e.ColumnIndex != 0)
                celdaActual = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dgvPrestaciones_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // validar que el cambio no sea el checkbox
            if (e.ColumnIndex != 0)
            {
                object objetoFinal = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                bool dif = false;
                if (celdaActual != null)
                    dif = !celdaActual.Equals(objetoFinal);
                else
                    dif = celdaActual != objetoFinal;

                if (dif)
                {
                    //MessageBox.Show("Hay cambios");
                    if (!filasActualizadas.Contains(e.RowIndex))
                    {
                        filasActualizadas.Add(e.RowIndex);
                        comprobarCambiosPrestaciones();
                    }

                    // Cambiar el color de la celda en caso de que existan cambios
                    var estilo = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Style;
                    estilo.ForeColor = Color.Blue;
                    ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Style = estilo;
                }
            }
        }

        private void dgvDetallesPrefactura_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //var row = e.RowIndex;
            //// validar que el doble click no sea en la cabecera y no haya elementos sin guardar
            //if (row != -1 && filasActualizadas.Count == 0)
            //{
            //    try
            //    {
            //        int numeroPF = Convert.ToInt32(((DataGridView)sender).Rows[row].Cells[1].Value);
            //        string lineaDPF = ((DataGridView)sender).Rows[row].Cells[2].Value.ToString();
            //        object[] pkey = { numeroPF, lineaDPF };

            //        var detallePrefactura = mainController.findDetallePrefactura(pkey);
            //        if (detallePrefactura != null)
            //        {
            //            this.idDetallePrefacturaSeleccionada = pkey;
            //            this.filtrarDetallePrefactura = true;
            //            limpiarFiltrosPrestaciones();
            //            filtrarPrestaciones(true);
            //            llenarCabecerasPrestaciones(detallePrefactura);
            //            tabControlPreFactura.SelectedIndex = 3;
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //}
        }

        private void tabControlPrefactura_SelectedIndexChanged(object sender, EventArgs e)
        {
            //txtbxMensajes.Text = "";

            //if (mainController.probarConexion())
            //{
            //    try
            //    {
            //        var prefacturaSeleccionada = mainController.findPrefactura(this.idPrefacturaSeleccionada);
            //        if (prefacturaSeleccionada == null)
            //        {
            //            dgvDetallesPrefactura.Rows.Clear();
            //            limpiarCabecerasDetallesPrefacturas();

            //            // deshabilitar la creación de detalles cuando no hay prefactura seleccionada
            //            toolStripButton6.Enabled = false;
            //        }
            //        else
            //        {
            //            // habilitar la creación de detalles cuando hay prefactura seleccionada
            //            toolStripButton6.Enabled = true;
            //        }

            //        var detallePrefactura = mainController.findDetallePrefactura(this.idDetallePrefacturaSeleccionada);
            //        if (detallePrefactura == null)
            //        {
            //            limpiarCabecerasPrestaciones();
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //}
            //else
            //{
            //    txtbxMensajes.Text = "No se pudo establecer la conexión con el servidor." + "\r\n";
            //}
        }

        private void tabControlPreFactura_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = comprobarCambiosDetallesPrefactura() || comprobarCambiosPrestaciones();

            if (comprobarCambiosDetallesPrefactura() || comprobarCambiosPrestaciones())
                txtbxMensajes.Text = "Para cambiar de pestaña debe guardar los cambios realizados." + "\r\n";
        }

        // devuelve true si existen cambios, false en caso contrario
        // además habilita y dehabilita los controles necesarios 
        private bool comprobarCambiosDetallesPrefactura()
        {
            //if (filasActualizadas.Count > 0)
            //{
            //    tabControlPreFactura.TabPages[1].Select();
            //    btnGuardarCambiosPrefactura.Enabled = true;
            //    toolStripButtonEliminarDetallePrefactura.Enabled = false;
            //    btnAplicarFiltroLibros.Enabled = false;
            //    cmbBxCompannia.Enabled = false;
            //    toolStripButton6.Enabled = false;
            //    return true;
            //}
            //else
            //{
            //    btnGuardarCambiosPrefactura.Enabled = false;
            //    toolStripButtonEliminarDetallePrefactura.Enabled = true;
            //    btnAplicarFiltroLibros.Enabled = true;
            //    cmbBxCompannia.Enabled = true;
            //    toolStripButton6.Enabled = true;
            //    return false;
            //}
            return false;
        }

        // devuelve true si existen cambios, false en caso contrario
        // además habilita y dehabilita los controles necesarios 
        private bool comprobarCambiosPrestaciones()
        {
            //if (filasActualizadas.Count > 0)
            //{
            //    tabControlPreFactura.TabPages[3].Select();
            //    toolStripButton7.Enabled = true;
            //    toolStripButtonGuardarCambiosPrestaciones.Enabled = false;
            //    button2.Enabled = false;
            //    cmbBxCompannia.Enabled = false;
            //    return true;
            //}
            //else
            //{
            //    toolStripButton7.Enabled = false;
            //    toolStripButtonGuardarCambiosPrestaciones.Enabled = true;
            //    button2.Enabled = true;
            //    cmbBxCompannia.Enabled = true;
            //    return false;
            //}

            return false;
        }

        private int filtrarDetallesPrefactura()
        {
            //var datos = mainController.getDetallesPrefactura(
            //    this.idPrefacturaSeleccionada,
            //    checkBoxPrefacturas_descripcion.Checked,
            //    textBoxPrefacturas_descripcion.Text,
            //    checkBoxPrefacturas_cantidad.Checked,
            //    textBoxPrefacturas_cantidad_desde.Text,
            //    textBoxPrefacturas_cantidad_hasta.Text,
            //    checkBoxPrefacturas_precio.Checked,
            //    textBoxPrefacturas_precio_desde.Text,
            //    textBoxPrefacturas_precio_hasta.Text,
            //    checkBoxPrefacturas_unidad_medida.Checked,
            //    textBoxPrefacturas_unidad_medida.Text,
            //    checkBoxPrefacturas_importe.Checked,
            //    textBoxPrefacturas_importe_desde.Text,
            //    textBoxPrefacturas_importe_hasta.Text,
            //    checkBoxPrefacturas_id_item.Checked,
            //    textBoxPrefacturas_id_item.Text
            //    );
            //dgvDetallesPrefactura.Rows.Clear();

            //foreach (var item in datos)
            //{
            //    object[] row = {
            //        false,
            //        item.NUMERO_PF, // columnas ocultas para obtener pkey
            //        item.LINEA_DPF, // columnas ocultas para obtener pkey
            //        item.ID_ITEM,
            //        item.DESCRIPCION,
            //        item.UNIDAD_MEDIDA,
            //        item.CANTIDAD.ToString(),
            //        item.PRECIO.ToString(),
            //        item.IMPORTE.ToString()
            //        };
            //    dgvDetallesPrefactura.Rows.Add(row);
            //}
            //return datos.Count;
            return 0;
        }

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {

        }

        private void guardarCambiosDetallePrefactura()
        {
            //using (var db = mainController.getDbContext())
            //{

            //    foreach (var row in filasActualizadas)
            //    {
            //        var numeroPF = Convert.ToInt32(dgvDetallesPrefactura.Rows[row].Cells[1].Value);
            //        var lineaDPF = dgvDetallesPrefactura.Rows[row].Cells[2].Value.ToString();
            //        object[] pkeys = { numeroPF, lineaDPF };


            //        var idItem = dgvDetallesPrefactura.Rows[row].Cells[3].Value.ToString();
            //        var descripcion = dgvDetallesPrefactura.Rows[row].Cells[4].Value.ToString();
            //        var unidadMedida = dgvDetallesPrefactura.Rows[row].Cells[5].Value.ToString();
            //        var cantidad = Convert.ToDecimal(dgvDetallesPrefactura.Rows[row].Cells[6].Value);
            //        var precio = Convert.ToDecimal(dgvDetallesPrefactura.Rows[row].Cells[7].Value);
            //        var importe = Convert.ToDecimal(dgvDetallesPrefactura.Rows[row].Cells[8].Value);

            //        var detallePrefactura = db.ITP_DETALLE_PREFACTURA.Find(pkeys);

            //        if (detallePrefactura != null)
            //        {
            //            detallePrefactura.ID_ITEM = idItem;
            //            detallePrefactura.DESCRIPCION = descripcion;
            //            detallePrefactura.UNIDAD_MEDIDA = unidadMedida;
            //            detallePrefactura.CANTIDAD = cantidad;
            //            detallePrefactura.PRECIO = precio;
            //            detallePrefactura.IMPORTE = importe;

            //            db.SaveChanges();
            //        }
            //    }

            //    filasActualizadas = new List<int>();
            //    filtrarDetallesPrefactura();
            //    comprobarCambiosDetallesPrefactura();
            //}
        }

        private void guardarCambiosPrestaciones()
        {
            //using (var db = mainController.getDbContext())
            //{
            //    foreach (var row in filasActualizadas)
            //    {
            //        var facturable = dgvPrestaciones.Rows[row].Cells["dgvPrestaciones_facturar"].Value.ToString();
            //        var modulo = dgvPrestaciones.Rows[row].Cells["dgvPrestaciones_tipo_modulo"].Value;
            //        var autorizacion = dgvPrestaciones.Rows[row].Cells["dgvPrestaciones_numero_autorizacion"].Value;

            //        var rowid = Convert.ToInt32(dgvPrestaciones.Rows[row].Cells["dgvPrestaciones_pk_rowid"].Value);
            //        var prestacion = db.ITP_PRESTACION.Where(m => m.ROWID == rowid).FirstOrDefault();

            //        if (prestacion.ITP_DETALLE_PREFACTURA == null)
            //        {
            //            if (prestacion != null)
            //            {
            //                prestacion.FACTURAR = facturable;
            //                prestacion.TIPO_MODULO = modulo != null ? modulo.ToString() : null;
            //                prestacion.NUMERO_AUTORIZACION = autorizacion != null ? autorizacion.ToString() : null;

            //                db.SaveChanges();
            //            }
            //        }
            //        else
            //        {
            //            txtbxMensajes.AppendText("No se puede actualizar la prestación número " + prestacion.ROWID + " porque está asociada a una factura." + "\r\n");
            //        }
            //    }

            //    filasActualizadas = new List<int>();
            //    filtrarPrestaciones(false);
            //    comprobarCambiosPrestaciones();
            //}
        }

        // Valida los campos de filtrado y devuelve los errores
        private string validarFiltrosPreFacturas()
        {
            string errores = "";
            if (checkBoxFecha.Checked)
            {
                if (dtPickerDesde.Value > dtPickerHasta.Value)
                    errores += "El campo fecha inicial debe ser menor que la fecha final."+Environment.NewLine;
            }

            return errores;
        }

        // Valida los campos de filtrado del tab Prefactura y devuelve los errores
        private string validarFiltrosPrefactura()
        {
            string errores = "";
            //if (cBoxYearLibro.Checked)
            //{
            //    string str1 = @"^([0-9]*)$";

            //    Regex re1 = new Regex(str1);
            //    if (!re1.IsMatch(tBoxYear.Text))
            //        errores += "El campo año debe ser un número entero.\r\n";
            //}
            return errores;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.filtrarDetallePrefactura = false;
                var c = filtrarPrestaciones(false);
                txtbxMensajes.Text = "";
                txtbxMensajes.AppendText("Filtros aplicados correctamente." + "\r\n");
                txtbxMensajes.AppendText("Total de resultados encontrados: " + c + "\r\n");
                limpiarCabecerasPrestaciones();
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = exc.Message + "\r\n";
            }
        }

        //private void llenarCabecerasDetallesPrefacturas(ITP_PREFACTURA prefactura)
        //{
        //    if (prefactura != null)
        //    {
        //        var prefacturaCliente = mainController.findPrefacturaCliente(prefactura.NUMERO_PF);
        //        if (prefacturaCliente != null)
        //        {
        //            textBoxPrefactura_nombre_cliente.Text = prefacturaCliente.nombreCliente;
        //            textBoxPrefactura_nombre_paciente.Text = prefacturaCliente.nombrePaciente;
        //        }
        //        textBoxPrefactura_tipo.Text = prefactura.TIPO_PF;
        //        textBoxPrefactura_numero.Text = prefactura.NUMERO_PF.ToString();
        //        textBoxPrefactura_fecha.Text = prefactura.FECHA.Value.ToShortDateString();
        //        textBoxPrefactura_id_cliente.Text = prefactura.ID_CLIENTE;

        //        textBoxPrefactura_fecha_desde.Text = prefactura.FECHA_DESDE.Value.ToShortDateString();
        //        textBoxPrefactura_estado.Text = prefactura.STATUS;
        //        textBoxPrefactura_referencia.Text = prefactura.REFERENCIA;
        //        textBoxPrefactura_total.Text = sumImporte(prefactura).ToString();
        //        textBoxPrefactura_id_paciente.Text = prefactura.ID_PACIENTE;

        //        textBoxPrefactura_fecha_hasta.Text = prefactura.FECHA_HASTA.Value.ToShortDateString();
        //        textBoxPrefactura_num_pref.Text = prefactura.SOPNUMBE;
        //    }
        //}

        //private void llenarCabecerasPrestaciones(ITP_DETALLE_PREFACTURA detallePrefactura)
        //{
        //    if (detallePrefactura != null)
        //    {
        //        textBoxPrestacionesCabeceraIdArticulo.Text = detallePrefactura.ID_ITEM;
        //        textBoxPrestacionesCabeceraDescripcion.Text = detallePrefactura.DESCRIPCION;
        //        textBoxPrestacionesCabeceraUdeM.Text = detallePrefactura.UNIDAD_MEDIDA;
        //        textBoxPrestacionesCabeceraCantidad.Text = detallePrefactura.CANTIDAD.ToString();
        //        textBoxPrestacionesCabeceraPrecio.Text = detallePrefactura.PRECIO.ToString();
        //        textBoxPrestacionesCabeceraImporte.Text = detallePrefactura.IMPORTE.ToString();
        //    }
        //}

        private void limpiarCabecerasDetallesPrefacturas()
        {
            //textBoxPrefactura_nombre_cliente.Text = "";
            //textBoxPrefactura_nombre_paciente.Text = "";

            //textBoxPrefactura_tipo.Text = "";
            //textBoxPrefactura_numero.Text = "";
            //textBoxPrefactura_fecha.Text = "";
            //textBoxPrefactura_id_cliente.Text = "";

            //textBoxPrefactura_fecha_desde.Text = "";
            //textBoxPrefactura_estado.Text = "";
            //textBoxPrefactura_referencia.Text = "";
            //textBoxPrefactura_total.Text = "";
            //textBoxPrefactura_id_paciente.Text = "";

            //textBoxPrefactura_fecha_hasta.Text = "";
            //textBoxPrefactura_num_pref.Text = "";
        }

        private void limpiarCabecerasPrestaciones()
        {
            //textBoxPrestacionesCabeceraIdArticulo.Text = "";
            //textBoxPrestacionesCabeceraDescripcion.Text = "";
            //textBoxPrestacionesCabeceraUdeM.Text = "";
            //textBoxPrestacionesCabeceraCantidad.Text = "";
            //textBoxPrestacionesCabeceraPrecio.Text = "";
            //textBoxPrestacionesCabeceraImporte.Text = "";
        }

        private void limpiarFiltrosPreFacturas()
        {
            checkBoxPacientes_numero_pf.Checked = false;
            checkBoxPacientes_nombre_cliente.Checked = false;
            checkBoxFecha.Checked = false;
            checkBoxEstado.Checked = false;
            checkBoxPacientes_sopnumbe.Checked = false;
            checkBoxPacientes_referencia.Checked = false;

            textBoxPacientes_numero_pf_desde.Text = "";
            textBoxPacientes_numero_pf_hasta.Text = "";
            textBoxPacientes_nombre_cliente.Text = "";
            dtPickerDesde.ResetText();
            dtPickerHasta.ResetText();
            cmbBEstado.SelectedIndex = 0;
            textBoxPacientes_sopnumbe_desde.Text = "";
            textBoxPacientes_sopnumbe_hasta.Text = "";
            textBoxPacientes_referencia.Text = "";
        }

        private void limpiarFiltrosPrestaciones()
        {
            //checkBoxPrestaciones_fecha.Checked = false;
            //checkBoxPrestaciones_prestacion.Checked = false;
            //checkBoxPrestaciones_modulo.Checked = false;
            //checkBoxPrestaciones_id_liquidacion.Checked = false;
            //checkBoxPrestaciones_nombre_prestador.Checked = false;
            //checkBoxPrestaciones_nombre_cliente.Checked = false;
            //checkBoxPrestaciones_nombre_paciente.Checked = false;

            //dateTimePickerPrestaciones_fecha_desde.ResetText();
            //dateTimePickerPrestaciones_fecha_hasta.ResetText();
            //textBoxPrestaciones_tipo_prestacion.Text = "";
            //textBoxPrestaciones_modulo.Text = "";
            //textBoxPrestaciones_nombre_prestador.Text = "";
            //textBoxPrestaciones_nombre_cliente.Text = "";
            //textBoxPrestaciones_nombre_paciente.Text = "";
            //textBoxPrestaciones_id_liquidacion.Text = "";
        }

        private void limpiarFiltrosDetallesPrefacturas()
        {
            //checkBoxPrefacturas_descripcion.Checked = false;
            //checkBoxPrefacturas_cantidad.Checked = false;
            //checkBoxPrefacturas_precio.Checked = false;
            //checkBoxPrefacturas_unidad_medida.Checked = false;
            //checkBoxPrefacturas_importe.Checked = false;
            //checkBoxPrefacturas_id_item.Checked = false;

            //textBoxPrefacturas_descripcion.Text = "";
            //textBoxPrefacturas_cantidad_desde.Text = "";
            //textBoxPrefacturas_cantidad_hasta.Text = "";
            //textBoxPrefacturas_precio_desde.Text = "";
            //textBoxPrefacturas_precio_hasta.Text = "";
            //textBoxPrefacturas_unidad_medida.Text = "";
            //textBoxPrefacturas_importe_desde.Text = "";
            //textBoxPrefacturas_importe_hasta.Text = "";
            //textBoxPrefacturas_id_item.Text = "";
        }

        //private decimal sumImporte(ITP_PREFACTURA prefactura)
        //{
        //    decimal importe = 0;
        //    foreach (var item in prefactura.ITP_DETALLE_PREFACTURA)
        //        importe += item.IMPORTE;
        //    return importe;
        //}

        private void btnAplicarFiltroLibros_Click(object sender, EventArgs e)
        {
            try
            {
                var c = filtrarDetallesPrefactura();
                txtbxMensajes.Text = "";
                txtbxMensajes.AppendText("Filtros aplicados correctamente." + "\r\n");
                txtbxMensajes.AppendText("Total de resultados encontrados: " + c + "\r\n");
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = exc.Message + "\r\n";
            }
        }

        private int filtrarPrestaciones(bool flag)
        {
            //var detallePrefactura = mainController.findDetallePrefactura(this.idDetallePrefacturaSeleccionada);
            //var datos = mainController.getPrestaciones(
            //    flag,
            //    detallePrefactura,
            //    checkBoxPrestaciones_fecha.Checked,
            //    dateTimePickerPrestaciones_fecha_desde.Value.Date.AddHours(0).AddMinutes(0).AddSeconds(0),
            //    dateTimePickerPrestaciones_fecha_hasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
            //    checkBoxPrestaciones_modulo.Checked,
            //    textBoxPrestaciones_modulo.Text,
            //    checkBoxPrestaciones_prestacion.Checked,
            //    textBoxPrestaciones_tipo_prestacion.Text,
            //    checkBoxPrestaciones_id_liquidacion.Checked,
            //    textBoxPrestaciones_id_liquidacion.Text,
            //    checkBoxPrestaciones_nombre_cliente.Checked,
            //    textBoxPrestaciones_nombre_cliente.Text,
            //    checkBoxPrestaciones_nombre_paciente.Checked,
            //    textBoxPrestaciones_nombre_paciente.Text,
            //    checkBoxPrestaciones_nombre_prestador.Checked,
            //    textBoxPrestaciones_nombre_prestador.Text
            //);

            //dgvPrestaciones.Rows.Clear();

            //foreach (var item in datos)
            //{
            //    object[] row = {
            //        false,
            //        item.ITP_PRESTACION.ROWID,                // columnas ocultas para obtener pkey
            //        item.ITP_PRESTACION.EMPRESA,              // columnas ocultas para obtener pkey
            //        item.ITP_PRESTACION.CODIGO_PRESTADOR,     // columnas ocultas para obtener pkey
            //        item.ITP_PRESTACION.CODIGO_NOVEDAD,       // columnas ocultas para obtener pkey
            //        item.ITP_PRESTACION.CODIGO_LIQUIDACION,   // columnas ocultas para obtener pkey
            //        item.ITP_PRESTACION.FECHA,                // columnas ocultas para obtener pkey

            //        item.ITP_PRESTACION.CODIGO_PRESTADOR,
            //        item.nombrePrestador,
            //        item.ITP_PRESTACION.FECHA,
            //        item.ITP_PRESTACION.TIPO_PRESTACION,
            //        item.ITP_PRESTACION.CANTIDAD,
            //        item.ITP_PRESTACION.UNIDAD_MEDIDA,
            //        item.ITP_PRESTACION.FACTURAR,
            //        item.ITP_PRESTACION.TIPO_MODULO,
            //        item.ITP_PRESTACION.NUMERO_AUTORIZACION,
            //        item.ITP_PRESTACION.CODIGO_CLIENTE,
            //        item.nombreCliente,
            //        item.ITP_PRESTACION.CODIGO_PACIENTE,
            //        item.nombrePaciente,
            //        item.ITP_PRESTACION.CODIGO_LIQUIDACION,
            //        item.ITP_PRESTACION.NUMERO_PF
            //    };
            //    dgvPrestaciones.Rows.Add(row);
            //}

            //foreach (DataGridViewRow item in dgvPrestaciones.Rows)
            //{
            //    if (item.Cells["dgvPrestaciones_numero_pf"].Value != null)
            //    {
            //        item.ReadOnly = true;
            //        item.Cells[0].ReadOnly = false;
            //    }
            //}

            //return datos.Count;
            return 0;
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            //marcarDesmarcarTodo(dgvPrestaciones);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            //var filas = getFilasSeleccionadas(dgvPrestaciones);
            //if (filas.Count > 0)
            //{
            //    var dialogResult = MessageBox.Show(this, "¿Confirma que desea eliminar los elementos seleccionados?", "Confirmación", MessageBoxButtons.OKCancel);
            //    if (dialogResult == DialogResult.OK)
            //    {
            //        txtbxMensajes.Text = "";

            //        var c = 0;
            //        foreach (DataGridViewRow item in filas)
            //        {
            //            var rowid = Convert.ToInt32(item.Cells["dgvPrestaciones_pk_rowid"].Value);
            //            var prestacion = mainController.findPrestacion(rowid);
            //            if (prestacion != null)
            //            {
            //                {
            //                    try
            //                    {
            //                        mainController.removePrestacion(rowid);
            //                        c++;
            //                    }
            //                    catch (Exception exc)
            //                    {
            //                        txtbxMensajes.AppendText("No se pudo eliminar la prestación con código de cliente \"" + prestacion.CODIGO_CLIENTE + "\" y prestador \"" + prestacion.CODIGO_PRESTADOR + "\"\r\n");
            //                        txtbxMensajes.AppendText(exc.Message + "\r\n");
            //                    }
            //                }
            //            }
            //        }

            //        if (c > 0)
            //        {
            //            var cant = filtrarPrestaciones(this.filtrarDetallePrefactura);

            //            txtbxMensajes.AppendText("Prestaciones eliminadas satisfactoriamente: " + c + "\r\n");
            //            txtbxMensajes.AppendText("Total de prestaciones existentes: " + cant + "\r\n");
            //        }
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(this, "No existen elementos seleccionados.", "Información");
            //}
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            var filas = getFilasSeleccionadas(dgvFacturas);
            //if (filas.Count > 0)
            //{
            //    var dialogResult = MessageBox.Show(this, "¿Confirma que desea eliminar los elementos seleccionados?", "Confirmación", MessageBoxButtons.OKCancel);
            //    if (dialogResult == DialogResult.OK)
            //    {
            //        txtbxMensajes.Text = "";

            //        var c = 0;
            //        foreach (DataGridViewRow item in filas)
            //        {
            //            var id = Convert.ToInt32(item.Cells[1].Value);
            //            var prefactura = mainController.findPrefactura(id);
            //            if (prefactura != null)
            //            {
            //                if (prefactura.STATUS.Equals("BORRADOR"))
            //                {
            //                    try
            //                    {
            //                        mainController.removePrefactura(id);
            //                        c++;
            //                    }
            //                    catch (Exception exc)
            //                    {
            //                        txtbxMensajes.AppendText("No se pudo eliminar la prefactura con número " + prefactura.NUMERO_PF + "\r\n");
            //                        txtbxMensajes.AppendText(exc.Message + "\r\n");
            //                    }
            //                }
            //                else
            //                {
            //                    txtbxMensajes.AppendText("No se pudo eliminar la prefactura con número " + prefactura.NUMERO_PF + "; el estado no es BORRADOR.\r\n");
            //                }
            //            }
            //        }

            //        if (c > 0)
            //        {
            //            var cant = filtrarPacientes();

            //            txtbxMensajes.AppendText("Prefacturas eliminadas satisfactoriamente: " + c + "\r\n");
            //            txtbxMensajes.AppendText("Total de prefacturas existentes: " + cant + "\r\n");
            //        }
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(this, "No existen elementos seleccionados.", "Información");
            //}
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            //var filas = getFilasSeleccionadas(dgvDetallesPrefactura);
            //if (filas.Count > 0)
            //{
            //    var dialogResult = MessageBox.Show(this, "¿Confirma que desea eliminar los elementos seleccionados?", "Confirmación", MessageBoxButtons.OKCancel);
            //    if (dialogResult == DialogResult.OK)
            //    {
            //        txtbxMensajes.Text = "";

            //        var c = 0;
            //        foreach (DataGridViewRow item in filas)
            //        {
            //            object[] pkey = { Convert.ToInt32(item.Cells[1].Value.ToString()), item.Cells[2].Value.ToString() };
            //            var detallePrefactura = mainController.findDetallePrefactura(pkey);
            //            if (detallePrefactura != null)
            //            {
            //                try
            //                {
            //                    mainController.removeDetallePrefactura(pkey);
            //                    c++;
            //                }
            //                catch (Exception exc)
            //                {
            //                    txtbxMensajes.AppendText("No se pudo eliminar el detalle de prefactura con número " + detallePrefactura.NUMERO_PF + "\r\n");
            //                    txtbxMensajes.AppendText(exc.Message + "\r\n");
            //                }
            //            }
            //        }


            //        if (c > 0)
            //        {
            //            var cant = filtrarDetallesPrefactura();

            //            txtbxMensajes.AppendText("Detalles de prefacturas eliminados satisfactoriamente: " + c + "\r\n");
            //            txtbxMensajes.AppendText("Total de detalles de prefacturas existentes: " + cant + "\r\n");
            //        }
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(this, "No existen elementos seleccionados.", "Información");
            //}
        }

        private async void prefacturar()
        {
            //var formGenerarPrefactura = new winformGenerarPrefactura(mainController);
            //formGenerarPrefactura.ShowDialog();

            //if (formGenerarPrefactura.DialogResult == DialogResult.OK)
            //{
            //    txtbxMensajes.Text = "";

            //    try
            //    {
            //        progressBar1.Style = ProgressBarStyle.Marquee;
            //        progressBar1.MarqueeAnimationSpeed = 40;

            //        txtbxMensajes.Text = "El proceso de facturación ha iniciado correctamente." + "\r\n";

            //        int c = await mainController.crearPrefactura(cmbBxCompannia.SelectedItem.ToString().Split('/')[1]);
            //        if (c > 0)
            //            txtbxMensajes.AppendText("Prefacturas generadas satisfactoriamente." + "\r\n");


            //        txtbxMensajes.Text = "El proceso de facturación ha finalizado correctamente." + "\r\n";

            //        filtrarPacientes();
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //    finally
            //    {
            //        progressBar1.Style = ProgressBarStyle.Continuous;
            //        progressBar1.MarqueeAnimationSpeed = 0;
            //    }
            //}
        }

        private void toolStripButton2_Click_2(object sender, EventArgs e)
        {
            //marcarDesmarcarTodo(dgvDetallesPrefactura);
        }

        private void btnGuardarCambiosPrefactura_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";

            try
            {
                guardarCambiosDetallePrefactura();
                txtbxMensajes.AppendText("Elementos actualizados satisfactoriamente" + "\r\n");
            }
            catch (Exception exc)
            {
                txtbxMensajes.AppendText("No se pudo realizar la operación, verifique el formato de los valores introducidos." + "\r\n" + exc.Message);
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";

            try
            {
                guardarCambiosPrestaciones();
                txtbxMensajes.AppendText("Elementos actualizados satisfactoriamente" + "\r\n");
            }
            catch (Exception exc)
            {
                txtbxMensajes.AppendText("No se pudo realizar la operación, verifique el formato de los valores introducidos." + "\r\n" + exc.Message);
            }
        }

        private void dgvPrestaciones_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //if (dgvPrestaciones.IsCurrentCellDirty)
            //{
            //    dgvPrestaciones.CommitEdit(DataGridViewDataErrorContexts.Commit);

            //}

        }

        private void dgvDetallesPrefactura_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //if (dgvDetallesPrefactura.IsCurrentCellDirty)
            //{
            //    dgvDetallesPrefactura.CommitEdit(DataGridViewDataErrorContexts.Commit);
            //}
        }

        private void dgvPacientes_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvFacturas.IsCurrentCellDirty)
            {
                dgvFacturas.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Devuelve las filas seleccionadas del grid especificado
        private List<DataGridViewRow> getFilasSeleccionadas(DataGridView dgv)
        {
            var listado = new List<DataGridViewRow>();
            foreach (DataGridViewRow item in dgv.Rows)
            {
                if (Boolean.Parse(item.Cells[0].Value.ToString()))
                    listado.Add(item);
            }

            return listado;
        }

        // Marca/desmarca todos los checks del grid especificado
        private void marcarDesmarcarTodo(DataGridView dgv)
        {
            bool value = false;
            bool flag = false;

            foreach (DataGridViewRow item in dgv.Rows)
            {
                if (!flag)
                {
                    value = !(Boolean.Parse(dgv.Rows[0].Cells[0].Value.ToString()));
                    flag = true;
                }
                item.Cells[0].Value = value;

                // Console.WriteLine(((DataGridViewCheckBoxCell)item.Cells[0]));
            }
            dgv.Refresh();
        }


        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            marcarDesmarcarTodo(dgvFacturas);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            prefacturar();
        }

        private void tsBtnExportaExcel_Click(object sender, EventArgs e)
        {
        }


        private void toolStripButton6_Click_1(object sender, EventArgs e)
        {
            //var formGenerarDetalle = new winformGenerarDetalle(mainController, idPrefacturaSeleccionada);
            //formGenerarDetalle.ShowDialog();

            //if (formGenerarDetalle.DialogResult == DialogResult.OK)
            //{
            //    filtrarDetallesPrefactura();
            //    txtbxMensajes.Text = "Detalle de prefactura creado correctamente." + "\r\n";
            //}
        }

        void CambiaEstado(int idLog, string docStatus, string transicion)
        {
            configuracion.GetParametros(cmbBxCompannia.SelectedIndex);
            IntegraVentasBandejaDB IntegraSOP = new IntegraVentasBandejaDB(configuracion, Environment.UserName);
            IntegraSOP.eventoProgreso += new IntegraVentasBandejaDB.LogHandler(reportaProgreso);
            IntegraSOP.ProcesaBandejaDB(idLog, docStatus, transicion);

        }

        private void tsMenuItemCambiarAListo_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            if (dgvFacturas.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
            }
            else 
            try
            {
                string idLog = dgvFacturas.SelectedRows[0].Cells[1].Value.ToString();
                string docStatus = dgvFacturas.SelectedRows[0].Cells[8].Value.ToString();
                this.CambiaEstado(short.Parse( idLog), docStatus, "ELIMINA_FACTURA_EN_GP");
                filtrarPreFacturas();
                reportaProgreso(0, "Proceso finalizado.");
                }
                catch (Exception gr)
            {

                reportaProgreso(0, gr.Message);
            }                

        }
    }
}
