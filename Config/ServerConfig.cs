using GameCore.Core;

namespace GameCore.Config;

public class ServerConfig
{
    // Network
    public string Ip {get; set;} ="127.0.0.1";
    public int Port {get; set;} =7171;
    public int MaxConnections {get; set;} =100;
    public int MaxPacketSize {get; set;} =1200;
    public int MaxPacketsPerSecond { get; set; } = 25;

    // Database 
    public string DatabasHost {get; set;} ="";
    public int DatabasePort {get; set;} = 5432;
    public string DatabasUser {get; set;} ="";
    public string DatabasPass {get; set;} ="";
    public string DatabasName {get; set;} ="";

    // misc
    public string WorldName {get; set;} ="DefaultWorld";
    public string Motd {get; set;} ="";
    public int TicksPerSecond {get; set;} =20;

    // connectionstring builder
    public string GetConnectionString()
    {
        return $"Host={DatabasHost};Port={DatabasePort};Database={DatabasName};Username={DatabasUser};Password={DatabasPass};";
    }

    //load config from file
    public static ServerConfig Load(string filePath = "config.cfg")
    {
        var config = new ServerConfig();

        if (!File.Exists(filePath))
        {
            Logger.Log(LogLevel.Debug,"CONFIG",$"Warning: {filePath} not found, using defaults..");
            return config;
        }

        Logger.Log(LogLevel.Debug,"CONFIG",$"Loading config from {filePath}");

        foreach (var line in File.ReadAllLines(filePath))
        {
            //Skip empty rows and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
            
            // Format : Key = value
            var parts = line.Split('=', 2);
            if(parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts [1].Trim();

            switch(key)
            {
                case "ip":
                    config.Ip = value;
                    break;

                case "port":
                    config.Port = int.Parse(value);
                    break;

                case "maxconnections":
                    config.MaxConnections = int.Parse(value);
                    break;
                
                case "MaxPacketSize":
                    config.MaxPacketSize = int.Parse(value);
                    break;
                
                case "MaxPacketsPerSecond":
                    config.MaxPacketsPerSecond = int.Parse(value);
                    break;

                case "database_host":
                    config.DatabasHost = value;
                    break;
                
                case "database_port":
                    config.DatabasePort = int.Parse(value);
                    break;

                case "database_user":
                    config.DatabasUser = value;
                    break;

                case "database_pass":
                    config.DatabasPass = value;
                    break;

                case "database_name":
                    config.DatabasName = value;
                    break;

                case "world_name":
                    config.WorldName = value;
                    break;

                case "motd":
                    config.Motd = value;
                    break;

                case "ticks_per_second":
                    config.TicksPerSecond = int.Parse(value);
                    break;
            }
        }

        return config;
    }
}