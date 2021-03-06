﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OCRMSupportForce {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class BatchSOQLQueries {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BatchSOQLQueries() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OCRMSupportForce.BatchSOQLQueries", typeof(BatchSOQLQueries).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to select p.Id, p.recieved_date, p.amount, f.Description
        /// from ocrm.payments p LEFT join ocrm.fund f 
        ///on p.FundID = f.Id
        /// where p.DonorId = @DONORIDPARAM 
        ///and p.recieved_date between &apos;2014-10-01&apos; and &apos;2015-09-30&apos;.
        /// </summary>
        internal static string FiveKDetailQ {
            get {
                return ResourceManager.GetString("FiveKDetailQ", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to select d.Id, d.name, d.Phone, d.email, d.streetaddress, d.city, d.state, d.zipcode,
        ///sum(p.amount) as Donor_Subtotal from ocrm.donors d 
        ///join ocrm.payments p on d.Id = p.DonorID 
        ///LEFT join ocrm.fund f on f.id = p.FundID
        /// where p.Recieved_Date between &apos;2014-10-01&apos; and &apos;2015-09-30&apos;
        ///group by d.Id
        /// Having sum(p.amount) &gt;= 5000 
        ///order by d.name.
        /// </summary>
        internal static string FiveKQ {
            get {
                return ResourceManager.GetString("FiveKQ", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT ID, Name, causeview__Name__c FROM
        ///causeview__Gift_Batch__c
        ///WHERE causeview__Close_Date__c = null
        ///and IsDeleted = false ORDER BY CREATEDDATE DESC.
        /// </summary>
        internal static string SelectOpenBatches {
            get {
                return ResourceManager.GetString("SelectOpenBatches", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT CreatedDate,Description,Id,OwnerId,Subject,WhoId FROM Task
        ///where CreatedDate &gt;= 2016-01-01T00:00:00Z
        ///and CreatedDate &lt;= 2016-12-31T23:59:00Z
        ///and subject = &apos;Call&apos;.
        /// </summary>
        internal static string Task2015Q {
            get {
                return ResourceManager.GetString("Task2015Q", resourceCulture);
            }
        }
    }
}
