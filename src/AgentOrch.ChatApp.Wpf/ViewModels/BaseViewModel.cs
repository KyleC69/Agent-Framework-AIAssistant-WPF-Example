using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;



namespace AgentOrch.ChatApp.Wpf.ViewModels;


public class BaseViewModel : ObservableObject, INotifyCollectionChanged, INotifyPropertyChanging
{


    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}