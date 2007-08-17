using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Drag_n_Unit
{
    public class ElementoNUnit
    {
        #region "Propiedades y variables"
        private string sTipo;
        private string sNombre;
        private string sDescripcion;
        private string sCodigo;

        public string Tipo
        {
            get { return sTipo; }
        }
        public string Nombre
        {
            get { return sNombre; }
        }
        public string Descripcion
        {
            get { return sDescripcion; }
        }
        public string Codigo
        {
            get { return sCodigo; }
        }
        #endregion
        public ElementoNUnit(string Tipo, string Nombre, string Descripcion, string Codigo)
        {
                sTipo = ValidoTexto(Tipo);
                sNombre = ValidoTexto(Nombre);
                sDescripcion = ValidoTexto(Descripcion);
                sCodigo = ValidoTexto(Codigo);
        }

        private string ValidoTexto(string texto)
        {
            string sRetorno="";
            if (texto.Trim().Length > 0)
                sRetorno = texto;
            else
                sRetorno = "No Definido";
            return sRetorno;
        }
    }
    public class CElementosNUnit : CollectionBase
    {
        public virtual void Add(ElementoNUnit Elemento)
        {
            this.List.Add(Elemento);
        }
        //indexador (read-only o sea hello)
        public virtual ElementoNUnit this[int Index]
        {
            get { return (ElementoNUnit)this.List[Index]; }
        }

        public ElementoNUnit Find(string Metodo)
        {
            ElementoNUnit rElemento = null;            
            foreach (ElementoNUnit Ele in this)
            {
                if (Ele.Nombre == Metodo)
                {                    
                    rElemento = Ele;
                    break;
                }
            }
            return rElemento;
        }
    }
}
