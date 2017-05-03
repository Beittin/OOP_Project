namespace ScheduleMapper.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Coordinate")]
    public partial class Coordinate
    {
        [Key]
        public long ID { get; set; }
        
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(5)]
        public string Abbreviation { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }

        public override string ToString()
        {
            //return base.ToString();
            return Latitude + "," + Longitude;
        }
    }
}
