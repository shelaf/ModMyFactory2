//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.ModSettings.Serialization
{
    internal enum PropertyTreeType : byte
    {
        None = 0,
        Bool = 1,
        Number = 2,
        String = 3,
        List = 4,
        Dictionary = 5
    }
}
