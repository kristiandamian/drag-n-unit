using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Drag_n_Unit
{
    public class Ensamblado
    {
        public enum Tipo { Contructores, Propiedades, Metodos, Eventos };
        #region "Propiedades y variables"
        //Variables
        private Assembly Exe;
        //Propiedades
        private string sDireccion;
        public string Direccion
        {
            //set{ sDireccion = value; }
            get { return sDireccion; }
        }

        private string sNombreArchivo;
        public string NombreArchivo
        {
            get { return sNombreArchivo; }
        }
        /// <summary>
        /// Devuelve los tipos definidos en la assembly
        /// </summary>
        public Type[] Tipos
        {
            get { return Exe.GetTypes(); }
        }

        public string NombreClase
        {
            get { AssemblyName NombreAss = Exe.GetName(); return NombreAss.Name; }
        }
        #endregion
        /// <summary>
        /// Constructor de la clase que usa reflexion para obtener los datos de un Assembly
        /// </summary>
        /// <param name="Direccion">Ruta completa donde se encuentra el assembly</param>
        public Ensamblado(string Direccion)
        {
            if (File.Exists(Direccion))//si existe el archivo
            {
                string[] NombreFile = Direccion.Split('\\');
                sNombreArchivo = NombreFile[NombreFile.Length - 1];//guardo el nombre del archivo
                //guardo la informacion de la assembly
                Exe = typeof(Object).Module.Assembly;
                Exe = Assembly.LoadFrom(Direccion);
                sDireccion = Direccion;//guardo la ruta completa del assembly                
            }
        }

        /// <summary>
        /// Valido si un metodo es static (que no requiere que se haga una instancia de una
        /// clase
        /// </summary>
        /// <param name="NombreMetodo">Nombre del metodo a validar</param>
        /// <param name="tipo">Tipo de metodo que estamos validando(si es un constructor,metodo,evento o propiedad)</param>
        /// <returns>True si es un metodo statico</returns>
        public bool EsStatic(string NombreMetodo, Tipo tipo)
        {
            bool bRetorno = false;
            if (tipo == Tipo.Metodos)
            {
                foreach (Type t in this.Tipos)
                {
                    MethodInfo[] mInfo = t.GetMethods();
                    foreach (MethodInfo m in mInfo)
                    {
                        if (m.ToString() == NombreMetodo)
                        {
                            bRetorno = m.IsStatic;
                            break;
                        }
                    }
                }
            }
            return bRetorno;
        }

        public string Definicion(string NombreMetodo, Tipo tipo)
        {
            string sRetorno = "";
            C_Sharp Lenguaje = new C_Sharp();
            string[] NombrePropiedad;
            switch (tipo)
            {
                case Tipo.Contructores:
                    sRetorno = Lenguaje.Constructor(NombreClase, "Obj1");
                    break;
                case Tipo.Eventos:
                    sRetorno = Lenguaje.Evento("Obj1", NombreMetodo, "Obj1_" + NombreMetodo);
                    break;
                case Tipo.Metodos:
                    NombrePropiedad = NombreMetodo.Split(' ');
                    int x = 0;
                    string sMetodo = "";
                    foreach (string str in NombrePropiedad)
                    {
                        if (x > 0)
                            sMetodo += str;
                        x++;
                    }
                    sRetorno = Lenguaje.Metodo("Obj1." + sMetodo);
                    break;
                case Tipo.Propiedades:
                    NombrePropiedad = NombreMetodo.Split(' ');
                    sRetorno = Lenguaje.Propiedad("Obj1",
                        NombrePropiedad[NombrePropiedad.Length - 1]);
                    break;
            }
            return sRetorno;
        }
        public ArrayList ListaExcepciones()
        {
            ArrayList ExceptionTree = new ArrayList();
            try
            {
                //añado al dominio la assembly 
                Assembly.Load(Exe.GetName());
                foreach (Assembly Assemblyes in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type Type in Assemblyes.GetTypes())
                    {
                        if (!Type.IsClass || !Type.IsPublic) continue;

                        StringBuilder TypeHierarchy =
                           new StringBuilder(Type.FullName, 5000);
                        Boolean IsDerivedFromException = false;
                        Type BaseType = Type.BaseType;
                        while ((BaseType != null) && !IsDerivedFromException)
                        {
                            TypeHierarchy.Append("-" + BaseType);
                            IsDerivedFromException = (BaseType == typeof(Exception));
                            BaseType = BaseType.BaseType;
                        }

                        if (!IsDerivedFromException) continue;

                        String[] Hierarchy = TypeHierarchy.ToString().Split('-');
                        Array.Reverse(Hierarchy);

                        /*ExceptionTree.Add(String.Join
                           ("-", Hierarchy, 1, Hierarchy.Length - 1));*/
                        ExceptionTree.Add(Hierarchy[Hierarchy.Length - 1]);
                        ExceptionTree.Sort();
                    }
                }
            }
            catch (Exception ex)
            {//
            }
            return ExceptionTree;
        }
    }
    //clases de apoyo para la de arriba
    public class Ensamblados : CollectionBase
    {
        public virtual void Add(Ensamblado Ensam)
        {
            this.List.Add(Ensam);
        }
        //indexador (read-only o sea hello)
        public virtual Ensamblado this[int Index]
        {
            get { return (Ensamblado)this.List[Index]; }
        }
        /// <summary>
        /// devuelve el indice del objeto donde se encuentra el metodo
        /// </summary>
        /// <param name="Metodo">Nombre del metodo</param>
        /// <returns>Indice de la coleccion donde se encuentra el metodo</returns>
        public int Find(string Metodo)
        {
            int iRetorno=-1;
            int iIndice = -1;
            bool bEncontro = false;
            foreach (Ensamblado Ensa in this)
            {
                foreach (Type tt in Ensa.Tipos)
                {
                    if (!bEncontro)
                    {
                        foreach (MethodInfo Metodos in tt.GetMethods())
                        {
                            if (Metodos.ToString() == Metodo)
                            {
                                bEncontro = true;
                                break;
                            }
                        }
                    }
                    if (!bEncontro)
                    {
                        foreach (PropertyInfo Propiedades in tt.GetProperties())
                        {
                            if (Propiedades.ToString() == Metodo)
                            {
                                bEncontro = true;
                                break;
                            }
                        }
                    }
                    if (!bEncontro)
                    {

                        //foreach (ConstructorInfo Constructores in tt.GetConstructors())
                        {
                            string[] NombreConstructor = tt.FullName.Split('.');
                            if (NombreConstructor[NombreConstructor.Length - 1] == Metodo)
                            {
                                bEncontro = true;
                                break;
                            }
                        }
                    }
                    if (!bEncontro)
                    {

                        foreach (EventInfo Eventos in tt.GetEvents())
                        {
                            if (Eventos.ToString() == Metodo)
                            {
                                bEncontro = true;
                                break;
                            }
                        }
                    }
                }
                iIndice++;
                if (bEncontro)
                {
                    iRetorno = iIndice;
                    break;
                }                
            }
            return iRetorno;
        }
    }

    public interface iLenguaje
    {
        /// <summary>
        /// Devuelvo la cadena que representa el constructor de un assembly
        /// </summary>
        /// <param name="Nombre">Nombre del Objeto</param>
        /// <returns></returns>
        string Constructor(string Nombre, string NombreInstancia);
        string Metodo(string Nombre, int NumeroComas);
        string Propiedad(string Objeto, string Nombre);
        string Evento(string Objeto, string Evento, string Funcion);
    }
    /// <summary>
    /// Genero esta clase para devolver las definiciones en un lenguaje definido (C#)
    /// para generar en otro lengua solo generar esta clase en el lenguaje definido
    /// </summary>
    public class C_Sharp : iLenguaje
    {
        public string Constructor(string Nombre,string NombreInstancia)
        {
            return Nombre.Trim()+" "+NombreInstancia.Trim()+ " = new "+Nombre.Trim()+"();";
        }

        //realmente lo puse solo para ser consistente con el nombre (METODO)
        public string Metodo(string Nombre)
        {
            return Nombre.Trim()+";";
        }
        public string Metodo(string Nombre,int NumeroComas)
        {
            string Comas="";
            for (int x = 0; x < NumeroComas; x++)
            {
                Comas += ",";
            }
            return Nombre.Trim() + "("+Comas+");";
        }

        public string Propiedad(string Objeto, string Nombre)
        {
            return Objeto.Trim() + "." + Nombre.Trim() + ";";
        }

        /// <summary>
        /// Devuelvo el codigo para añadir el manejador de un evento
        /// </summary>
        /// <param name="Objeto">Objeto definido</param>
        /// <param name="Evento">Nombre del evento (p.e. Click)</param>
        /// <param name="Funcion">Nombre de la funcion que manejara el evento (p.e. Forma_Click)</param>
        /// <returns></returns>
        public string Evento(string Objeto, string Evento,string Funcion)
        {
            return Objeto.Trim() + "." + Evento + " += new EventHandler(" + Funcion.Trim() + ");";            
        }
    }
}
