namespace WhatYouCarry.Core.Procgen;

/// <summary>One cell of the grid, by its block coordinates (D-78, D-234).</summary>
public readonly record struct Cell(int X, int Y, int Z);

/// <summary>One column of the grid: the X and the Z of a cell, over every row.</summary>
public readonly record struct Column(int X, int Z);
