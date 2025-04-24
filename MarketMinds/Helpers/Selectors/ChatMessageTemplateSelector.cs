using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Appointments;

namespace MarketMinds.Helpers.Selectors;

public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate MyTextMessageTemplate { get; set; }
    public DataTemplate TargetTextMessageTemplate { get; set; }
    public DataTemplate MyImageMessageTemplate { get; set; }
    public DataTemplate TargetImageMessageTemplate { get; set; }

    public int MyUserId { get; set; } = -1; // Default to invalid value

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is Message message && MyUserId != -1 && !string.IsNullOrEmpty(message.ContentType))
        {
            bool isMine = message.Creator == MyUserId;

            if (message.ContentType == "text")
            {
                return isMine ? MyTextMessageTemplate : TargetTextMessageTemplate;
            }
            else if (message.ContentType == "image")
            {
                return isMine ? MyImageMessageTemplate : TargetImageMessageTemplate;
            }
        }
        return base.SelectTemplateCore(item, container);
    }
    protected override DataTemplate SelectTemplateCore(object item)
    {
        return base.SelectTemplateCore(item);
    }
}
