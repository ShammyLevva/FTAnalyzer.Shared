/*
* AnselInputStreamReader<BR>
* This class reads an input stream of bytes representing
* ANSEL-encoded characters, and delivers a stream of UNICODE characters
* @Author: mhkay@iclway.co.uk
* @Version: 20 May 1998
*/

// 20 May 1998: conversion tables updated with input from John Cowan (cowan@locke.ccil.org)

namespace FTAnalyzer
{

    public class AnselInputStreamReader(Stream input) : StreamReader(input, new AnselEncoding())
    {
    }
}