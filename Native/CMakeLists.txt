cmake_minimum_required(VERSION 3.16)
project(aesgcmsiv)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Set output directories
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# Find OpenSSL
find_package(OpenSSL REQUIRED)

# Create the shared library
add_library(aesgcmsiv SHARED
    aesgcmsiv.cpp
    aesgcmsiv.h
)

# Link against OpenSSL
target_link_libraries(aesgcmsiv OpenSSL::Crypto)

# Set OpenSSL paths explicitly if provided
if(DEFINED OPENSSL_ROOT_DIR)
    set_target_properties(aesgcmsiv PROPERTIES
        LINK_FLAGS "/LIBPATH:${OPENSSL_ROOT_DIR}/lib"
    )
endif()

# Set library properties
set_target_properties(aesgcmsiv PROPERTIES
    OUTPUT_NAME "aesgcmsiv"
    PREFIX ""
    SUFFIX ".dll"
)

# Include directories
target_include_directories(aesgcmsiv PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${OPENSSL_INCLUDE_DIR}
)

# Compiler flags
if(MSVC)
    target_compile_options(aesgcmsiv PRIVATE /W4)
else()
    target_compile_options(aesgcmsiv PRIVATE -Wall -Wextra -Wpedantic)
endif()

# Windows-specific settings
if(WIN32)
    target_compile_definitions(aesgcmsiv PRIVATE
        WIN32_LEAN_AND_MEAN
        _CRT_SECURE_NO_WARNINGS
    )
endif() 