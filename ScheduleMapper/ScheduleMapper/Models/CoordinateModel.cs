namespace ScheduleMapper.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.ObjectModel;

    public partial class CoordinateModel : DbContext
    {
        public CoordinateModel()
            : base("name=CoordinateModel")
        {
        }

        public virtual DbSet<Coordinate> Coordinates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
