﻿
/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NPOI.Util
{
    /// <summary>
    /// This is similar to <see cref="RecordFormatException"/>, except this is thrown
    /// when there's a higher order problem with parsing a document beyond individual records.
    /// </summary>
    public class DocumentFormatException : RuntimeException
    {

        public DocumentFormatException(string exception)
            : base(exception)
        {

        }

        public DocumentFormatException(string exception, Exception ex)
            : base(exception, ex)
        {
            
        }

        public DocumentFormatException(Exception thr)
            : base(thr)
        {
            
        }

        /// <summary>
        /// Syntactic sugar to check whether a DocumentFormatException should
        /// be thrown.  If assertTrue is <c>false</c>, this will throw this
        /// exception with the message.
        /// </summary>
        /// <param name="assertTrue"></param>
        /// <param name="message"></param>
        public static void Check(bool assertTrue, string message)
        {
            if(!assertTrue)
            {
                throw new DocumentFormatException(message);
            }
        }
    }
}


