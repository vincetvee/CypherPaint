using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Windows.Media;

namespace DbAcceess
{
    [Table(nameof(CanvasWork))]
    public class  CanvasWork
    {
        public CanvasWork()
        {
            CanvasItems = new List<CanvasItemDetail>();
        }


        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime DateStored { get; set; }

        public virtual ICollection<CanvasItemDetail> CanvasItems { get; set; }
    }

    [Table(nameof(CanvasItemDetail))]
    public class CanvasItemDetail
    {


        [Key]
        public int Id { get; set; }

        [ForeignKey("CanvasWork")]
        public long CanvasWorkId { get; set; }

        public virtual CanvasWork CanvasWork { get; set; }

        public string ShapeType { get; set; }
        public int Index { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public double MinHeight { get; set; }

        public double MinWidth { get; set; }

        public string Data { get; set; }

        public double[] VisualOffset { get; set; }

        public double XOffSet { get; set; }
        public double YOffSet { get; set; }


        public string Stroke { get; set; }

        public string FillString { get; set; }

        public string Fill { get; set; }

    }


    public class AppDbContext: DbContext
    {
        public AppDbContext(): base("CypherDbConnection")
        {
            
                
        }

        public DbSet<CanvasWork> CanvasWorks { get; set; }

        public DbSet<CanvasItemDetail> CanvasItemDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


           
        }

    }
}
