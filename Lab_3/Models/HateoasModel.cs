using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Lab_3.Models {

    public abstract class HateoasModel {

        public dynamic _links = new ExpandoObject();
    }

    public class Link {

        public string href;

        public Link(string href) {
            this.href = href;
        }
    }
}