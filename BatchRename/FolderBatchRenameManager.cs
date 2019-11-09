﻿using BatchRename.UtilsClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BatchRename
{
    class FolderBatchRenameManager
    {

        private List<DirectoryInfo> FolderList;
        private List<string> NewFolderNames;

        private List<BatchRenameError> errors;
        public int DuplicateMode = 1;

        /// <summary>
        /// create manager to manage String Batch Renaming
        /// </summary>
        /// <param name="StringNames">names wanted to change</param>
        /// <param name="Operations">String operation wanted to perform on input names</param>
        public FolderBatchRenameManager()
        {

            errors = new List<BatchRenameError>();
            FolderList = new List<DirectoryInfo>();
            NewFolderNames = new List<string>();
        }

        public List<string> GetErrorList()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FolderList.Count; i++) //fill list with default vaule
            {
                result.Add("None");
            }
            for (int i = 0; i < errors.Count; i++)
            {
                int ErrorIndex = errors[i].NameErrorIndex;
                string Message = errors[i].Message;
                result[ErrorIndex] = Message;
            }
            return result;
        }

        private bool isInErrorList(int index)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (index == errors[i].NameErrorIndex)
                    return true;
            }
            return false;
        }

        public List<FolderObj> BatchRename(List<FolderObj> folderList, List<StringOperation> operations)
        {
            List<FolderObj> result = new List<FolderObj>(folderList);
            if (NewFolderNames.Count != 0) // clear list to save new changed names
            {
                NewFolderNames.Clear();
            }

            if (FolderList.Count != 0)
            {
                FolderList.Clear();
            }

            for (int i = 0; i < folderList.Count; i++)
            {
                string path = folderList[i].Path + "\\" + folderList[i].Name;
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FolderList.Add(directoryInfo);
                NewFolderNames.Add(directoryInfo.Name);
                Debug.WriteLine(directoryInfo.Name);
            }



            for (int i = 0; i < operations.Count; i++)
            {

                for (int j = 0; j < NewFolderNames.Count; j++)
                {
                    /*If the name is in error list, skip the rename process, to preserve the pre-error value*/
                    bool IsInErrorList = isInErrorList(j);
                    if (IsInErrorList)
                        continue;
                    try
                    {
                        NewFolderNames[j] = operations[i].OperateString(NewFolderNames[j]); // perform operation
                    }
                    catch (Exception e) //if operation has failed
                    {
                        BatchRenameError error = new BatchRenameError()
                        {
                            NameErrorIndex = j, // save the position of the string which caused the error
                            LastNameValue = NewFolderNames[j], //save the last values of the string before error
                            Message = e.Message, //the error message
                        };
                        errors.Add(error);
                    }
                }
            }

            //send back error messages that goes along with the folder name if there's one
            List<string> ErrorMessages = GetErrorList();

            for (int i = 0; i < folderList.Count; i++)
            {
                result[i].NewName = NewFolderNames[i];
                result[i].Error = ErrorMessages[i];

            }

            //if handling fails or user refuses to change
            if (handleDuplicateFolder() == false)
            {
                return result;
            };

            for (int i = 0; i < NewFolderNames.Count; i++)
            {
                if (result[i].NewName != NewFolderNames[i])
                {
                    result[i].NewName = NewFolderNames[i];
                    result[i].Error = "Name changed to avoid duplication";
                }
            }

            return result;
        }

        private bool handleDuplicateFolder()
        {
            //List<List<int>> DuplicatePositions = new List<List<int>>();
            //List<String> DuplicateVaules = new List<string>();

            //var duplicateKeys = NewFolderNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            //for (int i = 0; i <NewFolderNames.Count; i++)
            //{

            //}

            var duplicateKeys = NewFolderNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            if (duplicateKeys.Count == 0)
            {
                Debug.WriteLine("No values");
                return true;
            }

            //show duplicated names
            ChangesAlertDialog changesAlertDialog = new ChangesAlertDialog(duplicateKeys);
            if (changesAlertDialog.ShowDialog() != true)
            {
                return false;
            }

            for (int i = 0; i < NewFolderNames.Count; i++)
            {
                int count = 0;
                bool isDuplicate = true;
                string newName = NewFolderNames[i];

                //Change duplicated value till it's not the case
                while (isDuplicate)
                {
                    isDuplicate = false;

                    //check upper part of the list
                    for (int j = 0; j < i; j++)
                    {
                        if (newName == NewFolderNames[j])
                        {
                            isDuplicate = true;
                            count++;
                            newName = NewFolderNames[i] + "_" + count.ToString();
                        }
                    }

                    //check lower part of the list
                    for (int j = i + 1; j < NewFolderNames.Count; j++)
                    {
                        if (newName == NewFolderNames[j])
                        {
                            isDuplicate = true;
                            count++;
                            newName = NewFolderNames[i] + "_" + count.ToString();
                        }
                    }
                }
                NewFolderNames[i] = newName;
            }
            return true;
        }


        public void CommitChange()
        {

            for (int i = 0; i < FolderList.Count; i++)
            {
                string newPath = FolderList[i].Parent.FullName + "\\" + NewFolderNames[i];
                if (newPath != FolderList[i].FullName)
                    FolderList[i].MoveTo(newPath);
            }


        }
    }
}
