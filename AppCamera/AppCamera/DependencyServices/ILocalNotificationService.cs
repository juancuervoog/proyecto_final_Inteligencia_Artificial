using System;
using System.Collections.Generic;
using System.Text;

namespace AppCamera.DependencyServices
{
    public interface ILocalNotificationService
    {
        void LocalNotification(string title, string body, int id, DateTime notifyTime, string frecuencia, string tiempo);
        void Cancel(int id);
    }
}
