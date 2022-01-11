using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab_3.Models {

    public class CollectionModel<T> : HateoasModel {

        public List<T> _embedded;
    }
}