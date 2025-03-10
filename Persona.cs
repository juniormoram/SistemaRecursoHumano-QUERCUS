//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RHQuercus
{
    using System;
    using System.Collections.Generic;
    
    public partial class Persona
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Persona()
        {
            this.Aguinaldo = new HashSet<Aguinaldo>();
            this.EvaluacionDesem = new HashSet<EvaluacionDesem>();
            this.Incapacidad = new HashSet<Incapacidad>();
            this.Liquidacion = new HashSet<Liquidacion>();
            this.PermisoLaboral = new HashSet<PermisoLaboral>();
            this.Planilla = new HashSet<Planilla>();
            this.RegistroMarca = new HashSet<RegistroMarca>();
            this.Vacaciones = new HashSet<Vacaciones>();
        }
    
        public int IDCedula { get; set; }
        public string NombrePers { get; set; }
        public string Apellidos { get; set; }
        public string Direccion { get; set; }
        public int Celular { get; set; }
        public string Correo { get; set; }
        public Nullable<bool> Estado { get; set; }
        public int Ocupacion { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public Nullable<double> CantVacaciones { get; set; }
        public string NombreContacto { get; set; }
        public string ParentescoContacto { get; set; }
        public Nullable<int> CelularContacto { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Aguinaldo> Aguinaldo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EvaluacionDesem> EvaluacionDesem { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Incapacidad> Incapacidad { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Liquidacion> Liquidacion { get; set; }
        public virtual Ocupacion Ocupacion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PermisoLaboral> PermisoLaboral { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Planilla> Planilla { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RegistroMarca> RegistroMarca { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vacaciones> Vacaciones { get; set; }
    }
}
