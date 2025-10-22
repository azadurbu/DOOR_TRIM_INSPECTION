using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class WizardStep
    {
        public RECIPE_SETUP_STEPS Step { get; set; }
        public UserControl Content { get; set; }
        public int RecipeID { get; set; }
        public WizardStep(RECIPE_SETUP_STEPS Step, UserControl content)
        {
            this.Step = Step;
            this.Content = content;
        }
    }
    public class Wizard
    {
        private readonly List<WizardStep> _steps;
        private int _currentStepIndex;
        public Recipe Recipe = null;
        public Wizard()
        {
            _steps = new List<WizardStep>();
            _currentStepIndex = 0;
        }

        public void AddStep(WizardStep step)
        {
            _steps.Add(step);
        }

        public WizardStep GetCurrentStep()
        {
            if (_steps.Count == 0) return null;
            return _steps[_currentStepIndex];
        }

        public bool CanMoveNext()
        {
            return _currentStepIndex < _steps.Count - 1;
        }

        public bool CanMovePrevious()
        {
            return _currentStepIndex > 0;
        }

        public void MoveNext()
        {
            if (CanMoveNext()) _currentStepIndex++;
        }

        public void MovePrevious()
        {
            if (CanMovePrevious()) _currentStepIndex--;
        }

        public int StepCount => _steps.Count;
        public int CurrentStepIndex => _currentStepIndex;
    }

}
