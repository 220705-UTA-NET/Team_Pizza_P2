﻿using Project2_Server.Model;

namespace Project2_Server.API
{
    public class DTO_ProjectOrder
    {
        // FIELDS
        public DMODEL_Order INPUT_DMODEL_Order { get; set; }
        public List<DMODEL_Project> INPUT_LIST_DMODEL_Projects { get; set; }

        // CONSTRUCTORS
        public DTO_ProjectOrder () { }
        public DTO_ProjectOrder(DMODEL_Order INPUT_DMODEL_Order, List<DMODEL_Project> INPUT_LIST_DMODEL_Projects)
        {
            this.INPUT_DMODEL_Order = INPUT_DMODEL_Order;
            this.INPUT_LIST_DMODEL_Projects = INPUT_LIST_DMODEL_Projects;
        }

        // METHODS
    }
}
