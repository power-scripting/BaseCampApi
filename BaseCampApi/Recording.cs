﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaseCampApi {

	public enum Status { active, archived, trashed }

	/// <summary>
	/// The kinds of item you can access through the Recording API
	/// </summary>
	public class RecordingType {
		string _name;

		RecordingType(string name) {
			_name = name;
		}

		public static readonly RecordingType Comment = new RecordingType("Comment");
		public static readonly RecordingType Document = new RecordingType("Document");
		public static readonly RecordingType Message = new RecordingType("Message");
		public static readonly RecordingType Question = new RecordingType("Question::Answer");
		public static readonly RecordingType Schedule = new RecordingType("Schedule::Entry");
		public static readonly RecordingType Todo = new RecordingType("Todo");
		public static readonly RecordingType Todolist = new RecordingType("Todolist");
		public static readonly RecordingType Upload = new RecordingType("Upload");
		public static readonly RecordingType Vault = new RecordingType("Vault");
	}

	public enum DateSort { created_at, updated_at }

	public enum SortDirection { desc, asc }

	/// <summary>
	/// Used to access (and archive or trash) lots of different items.
	/// <see cref="RecordingType"/> for list of item types.
	/// </summary>
	public class Recording : CreatedItem {
		public bool visible_to_clients;
		static async public Task<ApiList<T>> GetAllRecordings<T>(Api api, RecordingType type, int projectId, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc)
			where T:new(){
			return await GetAllRecordings<T>(api, type, new int[] { projectId }, status, sort, direction);
		}

		static async public Task<ApiList<T>> GetAllRecordings<T>(Api api, RecordingType type, int[] projectIds = null, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc)
			where T:new() {
			return await api.GetAsync<ApiList<T>>(Api.Combine("projects", "recordings"), new {
				type,
				bucket = projectIds,
				status,
				sort,
				direction
			});
		}

		static async public Task SetStatus(Api api, int bucket, int id, Status status) {
			await api.PutAsync(Api.Combine("buckets", bucket, "recordings", id, "status", status));
		}

		async public Task SetStatus(Api api, Status status) {
			await SetStatus(api, bucket.id, id, status);
		}

	}

	/// <summary>
	/// Many recordings have a list of comments
	/// </summary>
	public class RecordingWithComments : Recording, ISubscribable {
		public string subscription_url;
		public int comments_count;
		public string comments_url;

		public string SubscriptionUrl => subscription_url;

		async public Task<ApiList<Comment>> GetComments(Api api) {
			if (comments_count == 0)
				return ApiList<Comment>.EmptyList(Api.UriToApi(comments_url));
			return await api.GetAsync<ApiList<Comment>>(Api.UriToApi(comments_url));
		}
	}
}
