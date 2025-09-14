import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';
import { MessageLookup } from '../lookup/message-lookup';
import {
  Message,
  MessagePersist,
  MessageReadPersist,
} from '../models/message/message.model';
import { nameof } from 'ts-simple-nameof';
import { Conversation } from '../models/conversation/conversation.model';
import { User } from '../models/user/user.model';
import { File } from '../models/file/file.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.messengerServiceAddress}api/messages`;
  }

  queryConversationMessages(
    q: MessageLookup,
    conversationId: string
  ): Observable<QueryResult<Message>> {
    const url = `${this.apiBase}/query/${conversationId}`;
    return this.http
      .post<QueryResult<Message>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Message> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .get<Message>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: MessagePersist, reqFields: string[] = []): Observable<Message> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<Message>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  read(item: MessageReadPersist[], reqFields: string[] = []): Observable<void> {
    const url = `${this.apiBase}/read`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<void>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }

  static getMessageFields(): string[] {
    return [
      nameof<Message>((x) => x.id),
      nameof<Message>((x) => x.content),
      nameof<Message>((x) => x.type),
      nameof<Message>((x) => x.status),
      nameof<Message>((x) => x.createdAt),
      nameof<Message>((x) => x.updatedAt),

      [
        nameof<Message>((x) => x.conversation),
        nameof<Conversation>((x) => x.id),
      ].join('.'),

      [nameof<Message>((x) => x.sender), nameof<User>((x) => x.fullName)].join(
        '.'
      ),

      [
        nameof<Message>((x) => x.sender),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),

      [nameof<Message>((x) => x.readBy), nameof<User>((x) => x.fullName)].join(
        '.'
      ),

      [
        nameof<Message>((x) => x.readBy),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
    ];
  }
}
