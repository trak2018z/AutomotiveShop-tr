﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AutomotiveShop.model
{
    public class CarDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CarDetailsId { get; set; }
        public string Producent { get; set; }
        public string Model { get; set; }
        public int YearOfProduction { get; set; }

        public virtual List<ProductByCar> ProductsByCar { get; set; }

        public CarDetails()
        {
            ProductsByCar = new List<ProductByCar>();
        }
    }
}