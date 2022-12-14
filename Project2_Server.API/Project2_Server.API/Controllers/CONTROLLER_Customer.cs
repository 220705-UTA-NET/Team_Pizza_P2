using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2_Server.Data;
using Project2_Server.Model;
using System.Security.Cryptography;


namespace Project2_Server.API.Controllers
{
    [Route("API/Customer")]
    [ApiController]
    public class CONTROLLER_Customer : ControllerBase
    {
        // FIELDS
        private readonly INTERFACE_SQL_Customer API_PROP_INTERFACE_Customer;
        private readonly INTERFACE_SQL_Order API_PROP_INTERFACE_Order;
        private readonly INTERFACE_SQL_Project API_PROP_INTERFACE_Project;
        private readonly INTERFACE_SQL_LinkingTable API_PROP_INTERFACE_LinkingTable;
        private readonly ILogger<CONTROLLER_Customer> API_DATA_Logger;

        // CONSTRUCTORS
        public CONTROLLER_Customer(INTERFACE_SQL_Customer INPUT_INTERFACE_Customer, INTERFACE_SQL_Order INPUT_INTERFACE_Order, INTERFACE_SQL_Project INPUT_INTERFACE_Project, INTERFACE_SQL_LinkingTable INPUT_INTERFACE_LinkingTable, ILogger<CONTROLLER_Customer> INPUT_Logger)
        {
            this.API_PROP_INTERFACE_Customer = INPUT_INTERFACE_Customer;
            this.API_PROP_INTERFACE_Order = INPUT_INTERFACE_Order;
            this.API_PROP_INTERFACE_Project = INPUT_INTERFACE_Project;
            this.API_PROP_INTERFACE_LinkingTable = INPUT_INTERFACE_LinkingTable;
            this.API_DATA_Logger = INPUT_Logger;
        }

        [HttpGet]
        [Route("CheckLogin")]
        public async Task<int> API_ASYNC_CUSTOMER_checkValidLogin(string INPUT_Email, string INPUT_Password)
        {
            try
            {
                // Verifing Inputs
                if (INPUT_Email == null || INPUT_Email == "" || INPUT_Password == null || INPUT_Password == "")
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_checkValidLogin --> OUTPUT: Invalid inputs");
                    return -1;
                }

                // Logic Implementation
                DMODEL_Customer WORK_DMODEL_Customer = await API_PROP_INTERFACE_Customer.CUSTOMER_ASYNC_checkCustomerLogin(INPUT_Email);

                if (WORK_DMODEL_Customer.customer_id == -1)
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_checkValidLogin --> OUTPUT: User doesn't exist in database");
                    return -1;
                }

                if (WORK_DMODEL_Customer.password == INPUT_Password)
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_checkValidLogin --> OUTPUT: User login verified");
                    return WORK_DMODEL_Customer.customer_id;
                }
                else
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_checkValidLogin --> OUTPUT: User password is incorrect");
                    return -1;
                }
            }
            catch (Exception e)
            {
                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_checkValidLogin --> OUTPUT: Failed to create new customer");
                API_DATA_Logger.LogError(e, e.Message);
                return -1;
            }
        }

        [HttpPost]
        [Route("CreateNewCustomer")]
        public async Task<bool> API_ASYNC_CUSTOMER_createNewCustomer([FromBody] DMODEL_Customer INPUT_DMODEL_Customer)
        {
            try
            {
                // Data Verification
                INPUT_DMODEL_Customer.DMODEL_CUSTOMER_verifyData();

                // Logic Implementation
                if (!await API_PROP_INTERFACE_Customer.CUSTOMER_ASYNC_createNewCustomer(INPUT_DMODEL_Customer))
                {
                    throw new Exception("Failed to create model");
                }

                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewCustomer --> OUTPUT: Created new customer");
                return true;
            }
            catch (Exception e)
            {
                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewCustomer --> OUTPUT: Failed to create new customer");
                API_DATA_Logger.LogError(e, e.Message);
                return false;
            }
        }

        [HttpPost]
        [Route("CreateNewOrder")]
        public async Task<bool> API_ASYNC_CUSTOMER_createNewOrder(int INPUT_CustomerID, [FromBody] DTO_OrderProject INPUT_DTO_OrderProject)
        {
            try
            {
                // Data Verification
                if (INPUT_CustomerID == null || INPUT_CustomerID < 0)
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Invalid input for customer ID");
                    return false;
                }
                
                INPUT_DTO_OrderProject.DTO_OrderProject_verifyData();

                // Logic Implementation
                // Adds Order into [Project2].[Order] Table
                int WORK_generatedOrderID = await API_PROP_INTERFACE_Order.ORDER_ASYNC_createNewOrder(INPUT_DTO_OrderProject.DMODEL_Order);

                if (WORK_generatedOrderID == -1)
                {
                    API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Recieved failed signal when creating new order, aborting task");
                    return false;
                }

                // Adds All Projects into [Project2].[Project] Table
                List<int> WORK_generatedProjectIDs = new List<int>();
                List<int> WORK_generatedProjectTypes = new List<int>();

                foreach (DMODEL_Project TEMP_Project in INPUT_DTO_OrderProject.LIST_DMODEL_Projects)
                {
                    int TEMP_generatedProjectID = await API_PROP_INTERFACE_Project.PROJECT_ASYNC_createNewProject(TEMP_Project);

                    if (TEMP_generatedProjectID == -1)
                    {
                        API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Recieved failed signal when creating new project for order {0}, aborting task", WORK_generatedOrderID);
                        return false;
                    }

                    WORK_generatedProjectIDs.Add(TEMP_generatedProjectID);
                    WORK_generatedProjectTypes.Add(TEMP_Project.item_id);
                }

                // Adds corresponding connection to each linking table
                await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToCustomerOrderLinkingTable(INPUT_CustomerID, WORK_generatedOrderID);

                for (int i = 0; i < WORK_generatedProjectIDs.Count; i++)
                {
                    await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToOrderProjectLinkingTable(WORK_generatedOrderID, WORK_generatedProjectIDs[i]);
                    switch(WORK_generatedProjectTypes[i])
                    {
                        case 1:
                            await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToProjectEmployeeLinkingTable_Chair(WORK_generatedProjectIDs[i]);
                            break;
                        case 2:
                            await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToProjectEmployeeLinkingTable_Table(WORK_generatedProjectIDs[i]);
                            break;
                        case 3:
                            await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToProjectEmployeeLinkingTable_Desk(WORK_generatedProjectIDs[i]);
                            break;
                    }
                }

                //foreach (int TEMP_ProjectID in WORK_generatedProjectIDs)
                //{
                //    await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_addToOrderProjectLinkingTable(WORK_generatedOrderID, TEMP_ProjectID);
                //}

                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Successfully created new order {0} for customer {1}", WORK_generatedOrderID, INPUT_CustomerID);

                return true;
            }
            catch (Exception e)
            {
                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Failed to create new order for customer {0}", INPUT_CustomerID);
                API_DATA_Logger.LogError(e, e.Message);
                return false;
            }
        }

        [HttpGet]
        [Route("GetOrderHistory")]
        public async Task<List<DTO_OrderProject>> API_ASYNC_CUSTOMER_getOrderHistory(int INPUT_CustomerID)
        {
            // Creates Blank / Dummy DTO_OrderProjects
            DTO_OrderProject DTO_Blank = new DTO_OrderProject();
            DMODEL_Order DMODEL_Order_Blank = new DMODEL_Order(-1, DateTime.MinValue, false);
            DTO_Blank.DMODEL_Order = DMODEL_Order_Blank;

            // Data Verification
            if (INPUT_CustomerID == null || INPUT_CustomerID < 0 )
            {
                List<DTO_OrderProject> OUTPUT_DTO_BLANK = new List<DTO_OrderProject>();
                OUTPUT_DTO_BLANK.Add(DTO_Blank);

                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_createNewOrder --> OUTPUT: Invalid input for customer ID");
                return OUTPUT_DTO_BLANK;
            }

            // Logic Implementation
            List<DTO_OrderProject> OUTPUT_DTO_OrderProject = new List<DTO_OrderProject>();

            List<int> WORK_LIST_OrderIDs = new List<int>();
            WORK_LIST_OrderIDs = await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_getFromCustomerOrderLinkingTable(INPUT_CustomerID);

            // Branch where Customer has no previous orders
            if (WORK_LIST_OrderIDs[0] == -1)
            {
                OUTPUT_DTO_OrderProject.Add(DTO_Blank);

                API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_getOrderHistory --> OUTPUT: Customer {0} has no order history, returning blank", INPUT_CustomerID);
                return OUTPUT_DTO_OrderProject;
            }

            // Branch where Customer has previous orders
            for (int i = 0; i < WORK_LIST_OrderIDs.Count; i++)
            {
                DTO_OrderProject TEMP_DTO_OrderProject = new DTO_OrderProject();
                TEMP_DTO_OrderProject.DMODEL_Order = await API_PROP_INTERFACE_Order.ORDER_ASYNC_getOrderData(WORK_LIST_OrderIDs[i]);

                List<int> WORK_LIST_ProjectIDs = await API_PROP_INTERFACE_LinkingTable.LINKING_ASYNC_getFromOrderProjectLinkingTable(WORK_LIST_OrderIDs[i]);
                List<DMODEL_Project> WORK_LIST_Projects = new List<DMODEL_Project>();

                for (int j = 0; j < WORK_LIST_ProjectIDs.Count; j++)
                {
                    DMODEL_Project TEMP_Project = await API_PROP_INTERFACE_Project.PROJECT_ASYNC_getProjectData(WORK_LIST_ProjectIDs[j]);
                    //API_DATA_Logger.LogInformation(TEMP_Project.project_id + " " + TEMP_Project.item_id + " " + TEMP_Project.completion_status);
                    if (TEMP_Project.project_id != -1)
                    {
                        WORK_LIST_Projects.Add(TEMP_Project);
                    }
                }

                TEMP_DTO_OrderProject.LIST_DMODEL_Projects = WORK_LIST_Projects;
                OUTPUT_DTO_OrderProject.Add(TEMP_DTO_OrderProject);
            }

            API_DATA_Logger.LogInformation("EXECUTED: API_ASYNC_CUSTOMER_getOrderHistory --> OUTPUT: found customer {0} order history, returning List of DTOs", INPUT_CustomerID);
            return OUTPUT_DTO_OrderProject;
        }
    }
}
