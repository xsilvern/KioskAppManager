﻿using System;
using System.Windows.Input;

public class RelayCommand : RelayCommandBase, ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action? execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentException(nameof(execute));
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }
    public void Execute(object? parameter)
    {
        _execute();
    }
}
public class RelayCommand<T> : RelayCommandBase, ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;
    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute((T?)parameter);
    }

    public void Execute(object? parameter)
    {
        _execute((T?)parameter);
    }
}
