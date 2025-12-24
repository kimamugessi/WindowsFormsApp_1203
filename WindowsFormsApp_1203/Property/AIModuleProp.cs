using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JYVision.Core;
using JYVision.SaigeSDK;

namespace JYVision.Property
{
    public partial class AIModuleProp : UserControl
    {
        SaigeAI _saigeAI; // SaigeAI 인스턴스
        string _modelPath = string.Empty;
        AIEngineType _engineType;
        public AIModuleProp()
        {
            InitializeComponent();

            cbAIModelType.DataSource = Enum.GetValues(typeof(AIEngineType)).Cast<AIEngineType>().ToList();
            cbAIModelType.SelectedIndex = 0;
        }

        private void btnSelAIModel_Click(object sender, EventArgs e)
        {

        }

        private void btnLoadModel_Click(object sender, EventArgs e)
        {

        }

        private void btnInspAI_Click(object sender, EventArgs e)
        {

        }

        private void cbAIModelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            AIEngineType engineType = (AIEngineType)cbAIModelType.SelectedItem;

            if (engineType != _engineType)
            {
                if (_saigeAI != null)
                    _saigeAI.Dispose();
            }

            _engineType = engineType;
        }
    }
}