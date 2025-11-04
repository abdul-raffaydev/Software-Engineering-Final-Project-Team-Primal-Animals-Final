using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

public interface IHeatMapService
{
    int GetPeakPressureIndex(PressureFrame frame);
    double GetContactAreaPercentage(PressureFrame frame);
}

