using System.Configuration;
using System.Data;
using System.Windows;

namespace KumanoKodo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static DataAccess? _dataAccess;

    public static DataAccess DataAccess
    {
        get
        {
            _dataAccess ??= new DataAccess();
            return _dataAccess;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Initialize database
        _ = DataAccess;
    }
}

