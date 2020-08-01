using LimFx.Business.Models;

namespace LimFxTemplate
{
    internal class DatabaseSettings : IBaseDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}