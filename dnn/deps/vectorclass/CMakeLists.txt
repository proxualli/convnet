cmake_minimum_required(VERSION 3.5.0)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

project(vectorclass VERSION 2 LANGUAGES CXX)

INCLUDE(GNUInstallDirs)

# Files
#--------------------------------------
set(SRC_FILES
	instrset.h	
	instrset_detect.cpp
	vectorclass.h
	vectorf128.h
	vectorf256.h
	vectorf256e.h
	vectorf512.h
	vectorf512e.h
	vectori128.h
	vectori256.h
	vectori256e.h
	vectori512.h
	vectori512e.h
	vectori512s.h
	vectori512se.h
	vectormath_common.h
	vectormath_exp.h
	vectormath_hyp.h
	vectormath_lib.h
	vectormath_trig.h
	vector_convert.h
	add-on/random/ranvec1.h
	add-on/random/ranvec1.cpp
	add-on/random/physseed.cpp
)

# Library
#--------------------------------------
add_library(${PROJECT_NAME} STATIC 
	${SRC_FILES}
)

target_include_directories(${PROJECT_NAME} PUBLIC  .)