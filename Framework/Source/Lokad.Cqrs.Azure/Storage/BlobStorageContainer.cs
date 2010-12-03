﻿#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
{
	/// <summary>
	/// Windows Azure implementation of storage 
	/// </summary>
	public sealed class BlobStorageRoot : IStorageRoot
	{
		readonly CloudBlobClient _client;
		readonly ILogProvider _provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageRoot"/> class.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="provider">The provider.</param>
		public BlobStorageRoot(CloudBlobClient client, ILogProvider provider)
		{
			_client = client;
			_provider = provider;
		}

		public IStorageContainer GetContainer(string name)
		{
			return new BlobStorageContainer(_client.GetBlobDirectoryReference(name));
		}
	}

	/// <summary>
	/// Windows Azure implementation of storage 
	/// </summary>
	public sealed class BlobStorageContainer : IStorageContainer
	{
		readonly CloudBlobDirectory _directory;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageContainer"/> class.
		/// </summary>
		/// <param name="directory">The directory.</param>
		public BlobStorageContainer(CloudBlobDirectory directory)
		{
			_directory = directory;
		}

		public IStorageContainer GetContainer([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new BlobStorageContainer(_directory.GetSubdirectory(name));
		}

		public IStorageItem GetItem([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			return new BlobStorageItem(_directory.GetBlobReference(name));
		}

		public IStorageContainer Create()
		{
			_directory.Container.CreateIfNotExist();
			return this;
		}

		public void Delete()
		{
			try
			{
				_directory.Container.Delete();
			}
			catch (StorageClientException e)
			{
				switch (e.ErrorCode)
				{
					case StorageErrorCode.ContainerNotFound:
						return;
					default:
						throw;
				}
			}
		}

		public bool Exists()
		{
			try
			{
				_directory.Container.FetchAttributes();
				return true;
			}
			catch (StorageClientException e)
			{
				switch (e.ErrorCode)
				{
					case StorageErrorCode.ContainerNotFound:
					case StorageErrorCode.ResourceNotFound:
					case StorageErrorCode.BlobNotFound:
						return false;
					default:
						throw;
				}
			}
		}

		public string FullPath
		{
			get { return _directory.Uri.ToString(); }
		}
	}
}