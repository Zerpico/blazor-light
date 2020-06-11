///
/// @file Clipboard.h
///
/// @brief The header for the Clipboard interface.
///
/// @author
///
/// This file is a part of Ultralight, a fast, lightweight, HTML UI engine
///
/// Website: <http://ultralig.ht>
///
/// Copyright (C) 2020 Ultralight, Inc. All rights reserved.
///
#pragma once
#include <Ultralight/Defines.h>
#include <Ultralight/String16.h>

namespace ultralight {

class UExport Clipboard {
public:
  virtual ~Clipboard();

  virtual void Clear() = 0;

  virtual String16 ReadPlainText() = 0;

  virtual void WritePlainText(const String16& text) = 0;
};

}  // namespace ultralight
