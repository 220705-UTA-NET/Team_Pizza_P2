namespace Project2_Server.Model
{
    // FIELDS
    public class DMODEL_Project
    {
        public int project_id { get; set; }
        public int item_id { get; set; }
        public bool completion_status { get; set; }

        // CONSTRUCTORS
        public DMODEL_Project() { }
        public DMODEL_Project(int INPUT_ProjectID, int INPUT_ItemID, bool INPUT_Status)
        {
            this.project_id = INPUT_ProjectID;
            this.item_id = INPUT_ItemID;
            this.completion_status = INPUT_Status;
        }

        // METHODS
        public void DMODEL_DEBUG_printCustomer()
        {
            Console.WriteLine(project_id);
            Console.WriteLine(item_id);
            Console.WriteLine(completion_status);
        }

        public void DMODEL_PROJECT_verifyData()
        {
            if (this.project_id == null || this.project_id < 0) throw new ArgumentNullException(nameof(this.project_id));
            if (this.item_id == null || this.item_id < 0) throw new ArgumentNullException(nameof(this.item_id));
            if (this.item_id == null) throw new ArgumentNullException(nameof(this.item_id));
        }
    }
}