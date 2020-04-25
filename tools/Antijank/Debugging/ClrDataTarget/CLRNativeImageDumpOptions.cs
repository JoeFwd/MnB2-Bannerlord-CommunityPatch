﻿using JetBrains.Annotations;

namespace Antijank.Debugging {

  [PublicAPI]
  public enum CLRNativeImageDumpOptions {

    CLRNATIVEIMAGE_PE_INFO = 1,

    CLRNATIVEIMAGE_COR_INFO,

    CLRNATIVEIMAGE_FIXUP_TABLES = 4,

    CLRNATIVEIMAGE_FIXUP_HISTOGRAM = 8,

    CLRNATIVEIMAGE_MODULE = 16,

    CLRNATIVEIMAGE_METHODS = 32,

    CLRNATIVEIMAGE_DISASSEMBLE_CODE = 64,

    CLRNATIVEIMAGE_IL = 128,

    CLRNATIVEIMAGE_METHODTABLES = 256,

    CLRNATIVEIMAGE_NATIVE_INFO = 512,

    CLRNATIVEIMAGE_MODULE_TABLES = 1024,

    CLRNATIVEIMAGE_FROZEN_SEGMENT = 2048,

    CLRNATIVEIMAGE_PE_FILE = 4096,

    CLRNATIVEIMAGE_GC_INFO = 8192,

    CLRNATIVEIMAGE_EECLASSES = 16384,

    CLRNATIVEIMAGE_NATIVE_TABLES = 32768,

    CLRNATIVEIMAGE_PRECODES = 65536,

    CLRNATIVEIMAGE_TYPEDESCS = 131072,

    CLRNATIVEIMAGE_VERBOSE_TYPES = 262144,

    CLRNATIVEIMAGE_METHODDESCS = 524288,

    CLRNATIVEIMAGE_METADATA = 1048576,

    CLRNATIVEIMAGE_DISABLE_NAMES = 2097152,

    CLRNATIVEIMAGE_DISABLE_REBASING = 4194304,

    CLRNATIVEIMAGE_SLIM_MODULE_TBLS = 8388608,

    CLRNATIVEIMAGE_RESOURCES = 16777216,

    CLRNATIVEIMAGE_FILE_OFFSET = 33554432,

    CLRNATIVEIMAGE_DEBUG_TRACE = 67108864,

    CLRNATIVEIMAGE_RELOCATIONS = 134217728,

    CLRNATIVEIMAGE_FIXUP_THUNKS = 268435456,

    CLRNATIVEIMAGE_DEBUG_COVERAGE = -2147483648

  }

}