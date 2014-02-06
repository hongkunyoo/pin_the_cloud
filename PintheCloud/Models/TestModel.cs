using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class TestModel
    {
        
        public InnerTestModel innerTestModel {get; set;}
        public string name {get; set;}
        public int id {get; set;}

        public TestModel()
        {
            innerTestModel = new InnerTestModel();
        }
        public override string ToString()
        {
            return name + id + innerTestModel.ToString();
        }
    }

    public class InnerTestModel
    {
        public string innerName { get; set; }
        public int innerId { get; set; }
        public override string ToString()
        {
            return innerName + innerId;
        }
    }
}
